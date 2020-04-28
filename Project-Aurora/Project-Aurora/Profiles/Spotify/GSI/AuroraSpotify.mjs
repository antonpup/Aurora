// START METADATA
// NAME: Aurora GSI
// AUTHOR: th3ant
// DESCRIPTION: Get RGB effects in Aurora
// END METADATA


/// <reference path= "../globals.d.ts" />

import ColorThief from './node_modules/colorthief/dist/color-thief.mjs';

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
                },
                "colorThief": {
                    "Main": {
                        "r": -1,
                        "g": -1,
                        "b": -1
                    },
                    "Palette": {
                        "color_1": {
                            "r": -1,
                            "g": -1,
                            "b": -1
                        },
                        "color_2": {
                            "r": -1,
                            "g": -1,
                            "b": -1
                        },
                        "color_3": {
                            "r": -1,
                            "g": -1,
                            "b": -1
                        },
                        "color_4": {
                            "r": -1,
                            "g": -1,
                            "b": -1
                        },
                        "color_5": {
                            "r": -1,
                            "g": -1,
                            "b": -1
                        }
                    }
                }
            },
            "track": {
                "album": "",
                "title": "",
                "artist": ""
            }
        }
    }

    update() {
        this.updatetimer = setInterval(() => {
            var self = this;

            const colorThief = new ColorThief();
            const img = new Image();
            const coverURL = Spicetify.Player.data.track.metadata.image_url.replace("spotify:image:", "https://i.scdn.co/image/");

            img.crossOrigin = 'Anonymous';
            img.src = coverURL;

            self.json.player.duration = Math.round(Spicetify.Player.getDuration() / 1000);
            self.json.player.progress = Math.round(Spicetify.Player.getProgress() / 1000);
            self.json.player.mute = Spicetify.Player.getMute();
            self.json.player.repeat = Spicetify.Player.getRepeat();
            self.json.player.shuffle = Spicetify.Player.getShuffle();
            self.json.player.heart = Spicetify.Player.getHeart();
            self.json.player.volume = Math.round(Spicetify.Player.getVolume() * 100);
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
            img.addEventListener('load', function () {
                var paletteColorArray = colorThief.getPalette(img, 5);
                var mainColorArray = colorThief.getColor(img);

                self.json.colors.colorThief.Main.r = mainColorArray[0] / 255 || 0;
                self.json.colors.colorThief.Main.g = mainColorArray[1] / 255 || 0;
                self.json.colors.colorThief.Main.b = mainColorArray[2] / 255 || 0;

                self.json.colors.colorThief.Palette.color_1.r = (paletteColorArray[0][0]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_1.g = (paletteColorArray[0][1]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_1.b = (paletteColorArray[0][2]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_2.r = (paletteColorArray[1][0]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_2.g = (paletteColorArray[1][1]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_2.b = (paletteColorArray[1][2]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_3.r = (paletteColorArray[2][0]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_3.g = (paletteColorArray[2][1]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_3.b = (paletteColorArray[2][2]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_4.r = (paletteColorArray[3][0]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_4.g = (paletteColorArray[3][1]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_4.b = (paletteColorArray[3][2]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_5.r = (paletteColorArray[4][0]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_5.g = (paletteColorArray[4][1]) / 255 || 0;
                self.json.colors.colorThief.Palette.color_5.b = (paletteColorArray[4][2]) / 255 || 0;


            });

            this.sendJsonToAurora(this.json);

        }, 1000);
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