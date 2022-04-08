//META{"name":"AuroraGSI","website":"http://www.project-aurora.com/","source":"https://github.com/Popat0/Discord-GSI/blob/master/AuroraGSI.plugin.js"}*//

/*@cc_on
@if (@_jscript)
    
    // Offer to self-install for clueless users that try to run this directly.
    var shell = WScript.CreateObject("WScript.Shell");
    var fs = new ActiveXObject("Scripting.FileSystemObject");
    var pathPlugins = shell.ExpandEnvironmentStrings("%APPDATA%BetterDiscordplugins");
    var pathSelf = WScript.ScriptFullName;
    // Put the user at ease by addressing them in the first person
    shell.Popup("It looks like you've mistakenly tried to run me directly. 
't do that!)", 0, "I'm a plugin for BetterDiscord", 0x30);
    if (fs.GetParentFolderName(pathSelf) === fs.GetAbsolutePathName(pathPlugins)) {
        shell.Popup("I'm in the correct folder already.", 0, "I'm already installed", 0x40);
    } else if (!fs.FolderExists(pathPlugins)) {
        shell.Popup("I can't find the BetterDiscord plugins folder.
you sure it's even installed?", 0, "Can't install myself", 0x10);
    } else if (shell.Popup("Should I copy myself to BetterDiscord's plugins folder for you?", 0, "Do you need some help?", 0x34) === 6) {
        fs.CopyFile(pathSelf, fs.BuildPath(pathPlugins, fs.GetFileName(pathSelf)), true);
        // Show the user where to put plugins in the future
        shell.Exec("explorer " + pathPlugins);
        shell.Popup("I'm installed!", 0, "Successfully installed", 0x40);
    }
    WScript.Quit();
@else@*/

function getModule (props) {
return BdApi.findModuleByProps.apply(null, props);
}


module.exports = class AuroraGSI {
    getName () {
        return 'AuroraGSI';
    }

    getDescription () {
        return 'Sends information to Aurora about users connecting to/disconnecting from, mute/deafen status';
    }

    getVersion () {
        return '2.4.1';
    }

    getAuthor () {
        return 'Popato & DrMeteor';
    }

    getChanges () {
        return {
        '1.0.0' :
                    `
                        Initial version.
                    `,
        '1.0.1' :
                    `
                        Added conditions for only reacting to local user.
                    `,
        '1.0.2' :
                    `
                        Removed isBeingCalled.
                        Removed redundant loop.
                    `,
        '1.0.3' :
                    `
                        Updated the CDN for the library.
                    `,
        '1.1' :
                    `
                        Made the state only be sent if it changed.
                    `,
        '2.0' :
                    `
                        Version bump to stop the update prompt derping.
                    `,
        '2.1.0':
                    `
                        Allow to track mute/deafen statuses outside voice channels.
                        Fix unread status for Enhanced Discord users.
                        Actually fix self-updating loop
                    `,
        '2.1.1':
                    `
                        Fix "being_called" boolean so it's now usable (triggers when user calls and getting called in DMs)
                    `,
        '2.2.0':
                    `
                        Rewrite a bunch of stuff
                    `,
        '2.3.0':
                    `
                        Rewrite some more stuff
                    `,
        '2.4.0':
                    `
                        Rewrite some more stuff
                    `,
		    '2.4.1':
                    `
                        Fix stuff for Canary
                    `
        };
    }

  constructor () {
    this.sendJsonToAurora = global._.debounce(this.sendJsonToAurora, 100);
  }

  getSelectedGuild () {
    return this.getGuild(getModule(['getLastSelectedGuildId'], false).getGuildId());
  }

  getSelectedTextChannel () {
    return this.getChannel(getModule(['getLastSelectedChannelId'], false).getChannelId());
  }

  getSelectedVoiceChannel () {
    return this.getChannel(getModule(['getLastSelectedChannelId'], false).getVoiceChannelId());
  }

  getLocalStatus () {
    return getModule([ 'guildPositions' ]).status;
  }

  load () {}// legacy

  start () {
    this.json = {
      provider: {
        name: 'discord',
        appid: -1
      },
      user:{
        id: -1,
        status: 'undefined',
        self_mute: false,
        self_deafen : false,
        mentions: false,
        mention_count: 0,
        unread_guilds_count: 0,
        unread_messages: false,
        being_called: false
      },
      guild: {
        id: -1,
        name: ''
      },
      text: {
        id: -1,
        type: -1,
        name: ''
      },
      voice: {
        id: -1,
        type: -1,
        name: ''
      }
    };
    // eslint-disable-next-line no-unused-expressions
    this.lastJson;
    this.getCurrentUser = getModule([ 'getUser', 'getUsers' ], false).getCurrentUser;
    this.getStatus = getModule([ 'getApplicationActivity' ], false).getStatus;
    this.getChannel = getModule([ 'getChannel', 'getDMFromUserId' ], false).getChannel;
    this.getGuild = getModule([ 'getGuild' ], false).getGuild;
    this.channels = getModule([ 'getChannelId' ], false);
    this.FluxDispatcher = getModule([ 'subscribe', 'dispatch' ], false);
    const { getUser } = getModule([ 'getUser' ], false),
      voice = getModule([ 'isMute', 'isDeaf', 'isSelfMute', 'isSelfDeaf' ], false),
      { getCalls } = getModule([ 'getCalls' ], false),
      { getMutableGuildStates: getUnreadGuilds } = getModule([ 'getMutableGuildStates' ], false),
      { getTotalMentionCount } = getModule([ 'getTotalMentionCount' ], false),
      isMute = voice.isMute.bind(voice),
      isDeaf = voice.isDeaf.bind(voice),
      isSelfMute = voice.isSelfMute.bind(voice),
      isSelfDeaf = voice.isSelfDeaf.bind(voice);
      /*
       * { getChannel } = getModule([ 'getChannel' ], false), // we dont use this yet
       * const { getVoiceStates } = getModule([ 'getVoiceState' ], false),
       */
    this.handler = (props) => {
      // eslint-disable-next-line consistent-this
      const localUser = this.getCurrentUser();
      const localStatus = this.getLocalStatus();
      /*
       * if (voiceChannel) {
       *   var voiceStates = getVoiceStates(voiceChannel.guild_id);
       * } not implemented
       */
      switch (props.type) {
        case 'PRESENCE_UPDATE':
          if (localUser && localStatus) {
            this.json.user.id = localUser?.id;
            this.json.user.status = localStatus;
          } else {
            this.json.user.id = -1;
            this.json.user.status = '';
          }
          break;

        case 'CHANNEL_SELECT':
          const guild = this.getGuild(props.guildId);
          const textChannel = this.getChannel(props.channelId);
          if (guild) {
            this.json.guild.id = guild.id;
            this.json.guild.name = guild.name;
          } else {
            this.json.guild.id = -1;
            this.json.guild.name = '';
          }
          if (textChannel) {
            this.json.text.id = textChannel.id;
            if (textChannel.type === 0) { // text channel
              this.json.text.type = 0;
              this.json.text.name = textChannel.name;
            } else if (textChannel.type === 5) { // announcement channel
              this.json.text.type = 5;
              this.json.text.name = textChannel.name;
            } else if (textChannel.type === 1) { // pm
              this.json.text.type = 1;
              this.json.text.name = getUser(textChannel.recipients[0]).username;
            } else if (textChannel.type === 3) { // group pm
              this.json.text.type = 3;
              if (textChannel.name) {
                this.json.text.name = textChannel.name;
              } else {
                let newname = '';
                for (let i = 0; i < textChannel.recipients.length; i++) {
                  const user = textChannel.recipients[i];
                  newname += `${getUser(user).username} `;
                }
                this.json.text.name = newname;
              }
            }
          } else {
            this.json.text.id = -1;
            this.json.text.type = -1;
            this.json.text.name = '';
          }
          break;

        case 'VOICE_CHANNEL_SELECT':
          const voiceChannel = this.getChannel(props.channelId);
          if (voiceChannel) {
            if (voiceChannel.type === 1) { // call
              this.json.voice.type = 1;
              this.json.voice.id = voiceChannel.id;
              this.json.voice.name = getUser(voiceChannel.recipients[0]).username;
            } else if (voiceChannel.type === 2) { // voice channel
              this.json.voice.type = 2;
              this.json.voice.id = voiceChannel.id;
              this.json.voice.name = voiceChannel.name;
            }
          } else {
            this.json.voice.id = -1;
            this.json.voice.type = -1;
            this.json.voice.name = '';
          }
          break;

        case 'USER_VOICE_UPDATE':
          this.json.user.self_mute = props.self_mute;
          this.json.user.self_deafen = props.self_deafen;
          this.json.user.mute = props.mute;
          this.json.user.deafen = props.deafen;
          break;

        case 'UNREADS_UPDATE':
          this.json.user.unread_messages = props.unreads > 0;
          this.json.user.unread_guilds_count = props.unreads;
          break;
        case 'MENTIONS_UPDATE':
          this.json.user.mentions = props.mentions > 0;
          this.json.user.mention_count = props.mentions;
          break;
        case 'CALL_RING_UPDATE':
          this.json.user.being_called = props.being_called;
          break;
        case 'SETUP':
          this.json.user.id = this.getCurrentUser()?.id;
          this.json.user.status = this.getLocalStatus;
          this.json.user.self_mute = isSelfMute();
          this.json.user.self_deafen = isSelfDeaf();
          this.json.user.mentions = getTotalMentionCount().length > 0;
          this.json.user.mention_count = getTotalMentionCount().length;
          this.json.user.unread_guilds_count = Object.values(getUnreadGuilds()).filter(obj => Object.values(obj).includes(true)).length;
          this.json.user.unread_messages = Object.values(getUnreadGuilds()).filter(obj => Object.values(obj).includes(true)).length > 0;
          break;
        default:
          break;
      }

      if (JSON.stringify(this.json) !== this.lastJson) {
        this.lastJson = JSON.stringify(this.json);
        this.sendJsonToAurora(this.json);
      }
    };

    const timeoutEventHandlers = () => {
      const voice = {};
      voice.self_mute = isSelfMute();
      voice.self_deafen = isSelfDeaf();
      voice.mute = isMute();
      voice.deafen = isDeaf();
      if (this.voice.mute !== voice.mute || this.voice.deafen !== voice.deafen) {
        this.handler({ type: 'USER_VOICE_UPDATE',
          ...voice });
        Object.assign(this.voice, voice);
      }
    };

    this.detectMention = (props) => {
      const uid = this.getCurrentUser()?.id;
      const mentions = getTotalMentionCount();
      if (props.message && !props.message.sendMessageOptions && props.message.author.id !== uid && this.mentions !== mentions) {
        this.handler({ type: 'MENTIONS_UPDATE',
          mentions });
        this.mentions = mentions;
      }
      const unreads = Object.values(getUnreadGuilds()).filter(obj => Object.values(obj).includes(true)).length;
      if (unreads !== this.unreads) {
        this.handler({ type: 'UNREADS_UPDATE',
          unreads });
        this.unreads = unreads;
      }
    };

    this.detectPresence = (props) => {
      if (props.user.id === this.getCurrentUser()?.id) {
        this.handler(props);
      }
    };

    this.detectCall = () => {
      setTimeout(() => {
        const being_called = (getCalls().filter((x) => x.ringing.length > 0).length > 0);
        if (being_called !== this.voice.being_called) {
          this.handler({ type: 'CALL_RING_UPDATE',
            being_called });
          this.voice.being_called = being_called;
        }
      }, 100);
    };
    this.FluxDispatcher.subscribe('MESSAGE_CREATE', this.detectMention);
    this.FluxDispatcher.subscribe('CHANNEL_SELECT', this.handler);
    this.FluxDispatcher.subscribe('VOICE_CHANNEL_SELECT', this.handler);
    this.FluxDispatcher.subscribe('PRESENCE_UPDATE', this.detectPresence);
    this.FluxDispatcher.subscribe('CALL_CREATE', this.detectCall);
    this.voice = {};
    this.unreads = 0;
    this.mentions = 0;
    this.interval = setInterval(timeoutEventHandlers, 100);
    const setupInterval = setInterval(() => {
      const u = this.getCurrentUser();
      if (u?.id) {
        clearInterval(setupInterval);
        this.handler({ type: 'SETUP' });
      }
    }, 100);
  }


  stop () {
    this.ready = false;
    clearInterval(this.interval);
    this.FluxDispatcher.unsubscribe('MESSAGE_CREATE', this.detectMention);
    this.FluxDispatcher.unsubscribe('CHANNEL_SELECT', this.handler);
    this.FluxDispatcher.unsubscribe('VOICE_CHANNEL_SELECT', this.handler);
    this.FluxDispatcher.unsubscribe('PRESENCE_UPDATE', this.detectPresence);
    this.FluxDispatcher.unsubscribe('CALL_CREATE', this.detectCall);
  }

  async sendJsonToAurora (json) {
    fetch('http://localhost:9088/', {
      method: 'POST',
      body: JSON.stringify(json),
      mode:'no-cors',
      headers:{
        'Content-Type': 'application/json'
      }
    })
      .catch(error => console.warn(`Aurora GSI error: ${error}`));
  }
};
