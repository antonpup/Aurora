//META{"name":"AuroraGSI","website":"http://www.project-aurora.com/","source":"https://github.com/Popat0/Discord-GSI/blob/master/AuroraGSI.plugin.js"}*//

/*@cc_on
@if (@_jscript)
	
	// Offer to self-install for clueless users that try to run this directly.
	var shell = WScript.CreateObject("WScript.Shell");
	var fs = new ActiveXObject("Scripting.FileSystemObject");
	var pathPlugins = shell.ExpandEnvironmentStrings("%APPDATA%\BetterDiscord\plugins");
	var pathSelf = WScript.ScriptFullName;
	// Put the user at ease by addressing them in the first person
	shell.Popup("It looks like you've mistakenly tried to run me directly. \n(Don't do that!)", 0, "I'm a plugin for BetterDiscord", 0x30);
	if (fs.GetParentFolderName(pathSelf) === fs.GetAbsolutePathName(pathPlugins)) {
		shell.Popup("I'm in the correct folder already.", 0, "I'm already installed", 0x40);
	} else if (!fs.FolderExists(pathPlugins)) {
		shell.Popup("I can't find the BetterDiscord plugins folder.\nAre you sure it's even installed?", 0, "Can't install myself", 0x10);
	} else if (shell.Popup("Should I copy myself to BetterDiscord's plugins folder for you?", 0, "Do you need some help?", 0x34) === 6) {
		fs.CopyFile(pathSelf, fs.BuildPath(pathPlugins, fs.GetFileName(pathSelf)), true);
		// Show the user where to put plugins in the future
		shell.Exec("explorer " + pathPlugins);
		shell.Popup("I'm installed!", 0, "Successfully installed", 0x40);
	}
	WScript.Quit();

@else@*/

class AuroraGSI {
	
    getName() { return "AuroraGSI"; }
    getDescription() { return "Sends information to Aurora about users connecting to/disconnecting from, mute/deafen status"; }
    getVersion() { return "1.1"; }
	getAuthor() { return "Popato & DrMeteor"; }
	getChanges() {
		return {
            "1.0.0" : 
            `
                Initial version.
            `,
            "1.0.1" :
            `
                Added conditions for only reacting to local user.
            `,
            "1.0.2" :
            `
                Removed isBeingCalled.
				Removed redundant loop.
            `,
            "1.0.3" :
            `
                Updated the CDN for the library.
            `,
            "1.1" :
            `
                Made the state only be sent if it changed.
            `
		};
    }
    
    constructor(){
        this.json = {
            "provider": {
                "name": "discord",
                "appid": -1
            },
            "user":{
                "id": -1,
                "status": "undefined",
                "self_mute": false,
                "self_deafen" : false,
                "mentions": false,
                "unread_messages": false,
                "being_called": false
            },
            "guild": {
                "id": -1,
                "name": "",
            },
            "text": {
                "id": -1,
                "type": -1,
                "name": "",
            },
            "voice": {
                "id": -1,
                "type": -1,
                "name": "",      
            }
        }
        this.lastJson;
    }

    load() {}//legacy

    start() {
        let libLoadedEvent = () => {
            try{ this.onLibLoaded(); }
            catch(err) { console.error(this.getName(), "fatal error, plugin could not be started!", err); }
        };

		let lib = document.getElementById("NeatoBurritoLibrary");
		if(lib == undefined) {
			lib = document.createElement("script");
			lib.setAttribute("id", "NeatoBurritoLibrary");
			lib.setAttribute("type", "text/javascript");
			lib.setAttribute("src", "https://raw.githack.com/Popat0/Discord-GSI/master/NeatoBurritoLibrary.js");
			document.head.appendChild(lib);
		}
        if(typeof window.Metalloriff !== "undefined") libLoadedEvent();
        else lib.addEventListener("load", libLoadedEvent);
    }
    
    stop() {
        clearInterval(this.updatetimer);
		//this.unpatch();
        this.ready = false;
    }
    
	onLibLoaded() {
        
        this.settings = NeatoLib.Settings.load(this, this.defaultSettings);
		NeatoLib.Updates.check(this, "https://raw.githubusercontent.com/Popat0/Discord-GSI/master/AuroraGSI.plugin.js");

        if(this.settings.displayUpdateNotes) NeatoLib.Changelog.compareVersions(this.getName(), this.getChanges());

        let getVoiceStates = NeatoLib.Modules.get(["getVoiceState"]).getVoiceStates,
            getUser = NeatoLib.Modules.get(["getUser"]).getUser,
            getChannel = NeatoLib.Modules.get(["getChannel"]).getChannel;
		
		// this.jsonTimer = setInterval( this.sendJsonToAurora, 50, this.json );

        this.updatetimer = setInterval(() => { 
            var self = this;
			
            var guild = NeatoLib.getSelectedGuild();
            var localUser = NeatoLib.getLocalUser();
            var localStatus = NeatoLib.getLocalStatus();
            var textChannel = NeatoLib.getSelectedTextChannel();
            var voiceChannel = NeatoLib.getSelectedVoiceChannel();
			if (voiceChannel)
				var voiceStates = getVoiceStates(voiceChannel.guild_id);

            if(localUser && localStatus){
                self.json.user.id = localUser.id;
                self.json.user.status = localStatus;
            }
            else {
                self.json.user.id = -1;
                self.json.user.status = "";
            }

            if(guild) {
                self.json.guild.id = guild.id;
                self.json.guild.name = guild.name;
            }
            else {
                self.json.guild.id = -1;
                self.json.guild.name = "";
            }

            if(textChannel){
                self.json.text.id = textChannel.id;
                if(textChannel.type === 0){//text channel
                    self.json.text.type = 0;
                    self.json.text.name = textChannel.name;
                }
                else if (textChannel.type === 1){//pm
                    self.json.text.type = 1;
                    self.json.text.name = getUser(textChannel.recipients[0]).username;
                }
                else if (textChannel.type === 3){//group pm
                    self.json.text.type = 3;
                    if(textChannel.name)
                        self.json.text.name = textChannel.name;
                    else{
                        let newname = "";
                        for(let i = 0; i< textChannel.recipients.length; i++){
                            let user = textChannel.recipients[i];
                            newname += getUser(user).username + " ";
                        }
                        self.json.text.name = newname;
                    }
                }
            }
            else
            {
                self.json.text.id = -1;
                self.json.text.type = -1;
                self.json.text.name = "";
            }

            if(voiceChannel){
                if(voiceChannel.type === 1){//call
                    self.json.voice.type = 1;
                    self.json.voice.id = voiceChannel.id;
                    self.json.voice.name = getUser(voiceChannel.recipients[0]).username;
                }
                else if(voiceChannel.type === 2) {//voice channel
                    self.json.voice.type = 2;
                    self.json.voice.id = voiceChannel.id;
                    self.json.voice.name = voiceChannel.name;
                }
            }
            else{
                self.json.voice.id = -1;
                self.json.voice.type = -1;    
                self.json.voice.name = "";
            }

            if(voiceStates){
                let userVoiceState = voiceStates[localUser.id];
                if(userVoiceState){
                    self.json.user.self_mute = userVoiceState.selfMute;
                    self.json.user.self_deafen = userVoiceState.selfDeaf;
                }        
            }
			
			self.json.user.unread_messages = false;
			self.json.user.mentions = false;
			
			if (document.querySelector('[class^="numberBadge-"]'))
				self.json.user.mentions = true;
			if (document.getElementsByClassName("bd-unread").length > 0)
                self.json.user.unread_messages = true;

            if(JSON.stringify(this.json) !== this.lastJson){
                console.log("false");
                this.lastJson = JSON.stringify(this.json);
                this.sendJsonToAurora (this.json);
            }			
        }, 100);
		
        NeatoLib.Events.onPluginLoaded(this);
    }

    async sendJsonToAurora(json) {
        fetch('http://localhost:9088/', {
            method: 'POST',
            body: JSON.stringify(json),
            mode:'no-cors',
            headers:{
                'Content-Type': 'application/json'
            }
        })
		.catch (error => {
			return undefined;
		});
    }
}
