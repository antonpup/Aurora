// START METADATA
// NAME: Aurora GSI
// AUTHOR: th3ant
// DESCRIPTION: Get RGB effects in Aurora
// END METADATA


/// <reference path="../globals.d.ts" />

class AuroraSpotify {
    
    constructor() {
        this.json = {
            "provider": {
                "name": "spotify",
                "appid": -1
            },
            "player":{
                "duration": -1,
                "progress": -1,
                "mute": false,
                "repeat": -1,
                "shuffle": false,
                "heart": false,
                "volume": -1,
                "playing": false
            },
            "colors": {
                "desaturated": {
                    "r":-1,
                    "g":-1,
                    "b":-1
                },
                "light_vibrant": {
                    "r":-1,
                    "g":-1,
                    "b":-1
                },
                "prominent": {
                    "r":-1,
                    "g":-1,
                    "b":-1
                },
                "vibrant": {
                    "r":-1,
                    "g":-1,
                    "b":-1
                },
                "vibrant_non_alarming": {
                    "r":-1,
                    "g":-1,
                    "b":-1
                }
            },
			"track":{
				"album": "",
				"title": "",
				"artist":""
			}
        }
    }

    update() {
        this.updatetimer = setInterval(() => {
        var self = this;

        self.json.player.duration = Math.round(Spicetify.Player.getDuration());
        self.json.player.progress = Math.round(Spicetify.Player.getProgress());
        self.json.player.mute = Spicetify.Player.getMute();
        self.json.player.repeat = Spicetify.Player.getRepeat();
        self.json.player.shuffle = Spicetify.Player.getShuffle();
        self.json.player.heart = Spicetify.Player.getHeart();
        self.json.player.volume = Math.round(Spicetify.Player.getVolume()*100);
        self.json.player.playing = Spicetify.Player.isPlaying();
		self.json.track.album = Spicetify.Player.data.track.metadata.album_title;
		self.json.track.artist = Spicetify.Player.data.track.metadata.artist_name;
		self.json.track.title = Spicetify.Player.data.track.metadata.title;
        Spicetify.getAblumArtColors(Spicetify.Player.data.track.metadata.album_uri)
        .then((colors) => {
            self.json.colors.desaturated = this.hexToRGB(colors.DESATURATED);;
            self.json.colors.light_vibrant = this.hexToRGB(colors.LIGHT_VIBRANT);
            self.json.colors.prominent = this.hexToRGB(colors.PROMINENT);
            self.json.colors.vibrant = this.hexToRGB(colors.VIBRANT);
            self.json.colors.vibrant_non_alarming = this.hexToRGB(colors.VIBRANT_NON_ALARMING);
        })

        this.sendJsonToAurora (this.json);

        }, 500);
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

    hexToRGB(hex){
        return {
            "r": parseInt(hex.slice(1, 3), 16)/255 || 0,
            "g": parseInt(hex.slice(3, 5), 16)/255 || 0,
            "b": parseInt(hex.slice(5, 7), 16)/255 || 0
        };
    }
}

let run = new AuroraSpotify();
run.update();