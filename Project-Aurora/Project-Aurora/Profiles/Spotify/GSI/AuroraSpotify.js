// START METADATA
// NAME: Aurora GSI
// AUTHOR: th3ant & diogotr7
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
            "player": {
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
                    "r": -1,
                    "g": -1,
                    "b": -1
                },
                "light_vibrant": {
                    "r": -1,
                    "g": -1,
                    "b": -1
                },
                "prominent": {
                    "r": -1,
                    "g": -1,
                    "b": -1
                },
                "vibrant": {
                    "r": -1,
                    "g": -1,
                    "b": -1
                },
                "vibrant_non_alarming": {
                    "r": -1,
                    "g": -1,
                    "b": -1
                }
            },
            "track": {
                "album": "",
                "title": "",
                "artist": ""
            }
        }

        this.lastJson = "";
    }

    update() {
        this.updatetimer = setInterval(() => {
            this.json.player.duration = Math.round(Spicetify.Player.getDuration() / 1000);
            this.json.player.progress = Math.round(Spicetify.Player.getProgress() / 1000);
            this.json.player.mute = Spicetify.Player.getMute();
            this.json.player.repeat = Spicetify.Player.getRepeat();
            this.json.player.shuffle = Spicetify.Player.getShuffle();
            this.json.player.heart = Spicetify.Player.getHeart();
            this.json.player.volume = Math.round(Spicetify.Player.getVolume() * 100);
            this.json.player.playing = Spicetify.Player.isPlaying();
            this.json.track.album = Spicetify.Player.data.track.metadata.album_title;
            this.json.track.artist = Spicetify.Player.data.track.metadata.artist_name;
            this.json.track.title = Spicetify.Player.data.track.metadata.title;
            Spicetify.getAblumArtColors(Spicetify.Player.data.track.metadata.album_uri).then((colors) => {
                this.json.colors.desaturated = this.hexToRGB(colors.DESATURATED);;
                this.json.colors.light_vibrant = this.hexToRGB(colors.LIGHT_VIBRANT);
                this.json.colors.prominent = this.hexToRGB(colors.PROMINENT);
                this.json.colors.vibrant = this.hexToRGB(colors.VIBRANT);
                this.json.colors.vibrant_non_alarming = this.hexToRGB(colors.VIBRANT_NON_ALARMING);
            })

            if (JSON.stringify(this.json) === this.lastJson) {
                //if nothing changes, skip
            }
            else {
                this.lastJson = JSON.stringify(this.json);
                this.sendJsonToAurora(this.json);
            }
        }, 100);
    }

    async sendJsonToAurora(json) {
        fetch('http://localhost:9088/', {
            method: 'POST',
            body: JSON.stringify(json),
            mode: 'no-cors',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .catch(error => {
                return undefined;
            });
    }

    hexToRGB(hex) {
        return {
            "r": parseInt(hex.slice(1, 3), 16) / 255 || 0,
            "g": parseInt(hex.slice(3, 5), 16) / 255 || 0,
            "b": parseInt(hex.slice(5, 7), 16) / 255 || 0
        };
    }
}

let run = new AuroraSpotify();
run.update();
