﻿(handler "AURORA"
    (lambda (data)
        (when (keyboard:? data)
            (let* ((keyboard (keyboard: data))
                   (hids (hids: keyboard))
                   (colors (colors: keyboard)))
                (on-device "rgb-per-key-zones" show-on-keys: hids colors)))
        (when (periph:? data)
            (let* ((periph (periph: data))
                   (color (color: periph)))
                (on-device "rgb-1-zone" show: color)
                (on-device "rgb-2-zone" show: color)
                (on-device "rgb-3-zone" show: color)
                (on-device "rgb-4-zone" show: color)
                (on-device "rgb-5-zone" show: color)
                (on-device "rgb-8-zone" show: color)
                (on-device "rgb-12-zone" show: color)
                (on-device "rgb-17-zone" show: color)
                (on-device "rgb-24-zone" show: color)
                (on-device "rgb-103-zone" show: color)))
        (when (onezone:? data)
            (let* ((onezone (onezone: data))
                    (colors (colors: onezone)))
                (on-device "rgb-1-zone" show-on-zones: colors '(one:))))
        (when (twozone:? data)
            (let* ((twozone (twozone: data))
                    (colors (colors: twozone)))
                (on-device "rgb-2-zone" show-on-zone: colors '(one: two:))))
        (when (threezone:? data)
            (let* ((threezone (threezone: data))
                    (colors (colors: threezone)))
                (on-device "rgb-3-zone" show-on-zones: colors '(one: two: three:))))
        (when (fourzone:? data)
            (let* ((fourzone (fourzone: data))
                    (colors (colors: fourzone)))
                (on-device "rgb-4-zone" show-on-zones: colors '(one: two: three: four:))))
        (when (fivezone:? data)
            (let* ((fivezone (fivezone: data))
                    (colors (colors: fivezone)))
                (on-device "rgb-5-zone" show-on-zones: colors '(one: two: three: four: five:))))
        (when (eightzone:? data)
            (let* ((eightzone (eightzone: data))
                    (colors (colors: eightzone)))
                (on-device "rgb-8-zone" show-on-zones: colors '(one: two: three: four: five: six: seven: eight:))))
        (when (twelvezone:? data)
            (let* ((twelvezone (twelvezone: data))
                    (colors (colors: twelvezone)))
                (on-device "rgb-12-zone" show-on-zones: colors '(one: two: three: four: five: six: seven: eight: nine: ten: eleven: twelve:))))
        (when (seventeenzone:? data)
            (let* ((seventeenzone (seventeenzone: data))
                    (colors (colors: seventeenzone)))
                (on-device "rgb-17-zone" show-on-zones: colors '(one: two: three: four: five: six: seven: eight: nine: ten: eleven: twelve: thirteen: fourteen: fifteen: sixteen: seventeen:))))
        (when (twentyfourzone:? data)
            (let* ((twentyfourzone (twentyfourzone: data))
                    (colors (colors: twentyfourzone)))
                (on-device "rgb-24-zone" show-on-zones: colors '(one: two: three: four: five: six: seven: eight: nine: ten: eleven: twelve: thirteen: fourteen: fifteen: sixteen: seventeen: eighteen: nineteen: twenty: twenty-one: twenty-two: twenty-three: twenty-four:))))
        (when (hundredthreezone:? data)
            (let* ((hundredthreezone (hundredthreezone: data))
                    (colors (colors: hundredthreezone)))
                (on-device "rgb-103-zone" show-on-zones: colors '(one: two: three: four: five: six: seven: eight: nine: ten: eleven: twelve: thirteen: fourteen: fifteen: sixteen: seventeen: eighteen: nineteen: twenty: twenty-one: twenty-two: twenty-three: twenty-four: twenty-five: twenty-six: twenty-seven: twenty-eight: twenty-nine: thirty: thirty-one: thirty-two: thirty-three: thirty-four: thirty-five: thirty-six: thirty-seven: thirty-eight: thirty-nine: forty: forty-one: forty-two: forty-three: forty-four: forty-five: forty-six: forty-seven: forty-eight: forty-nine: fifty: fifty-one: fifty-two: fifty-three: fifty-four: fifty-five: fifty-six: fifty-seven: fifty-eight: fifty-nine: sixty: sixty-one: sixty-two: sixty-three: sixty-four: sixty-five: sixty-six: sixty-seven: sixty-eight: sixty-nine: seventy: seventy-one: seventy-two: seventy-three: seventy-four: seventy-five: seventy-six: seventy-seven: seventy-eight: seventy-nine: eighty: eighty-one: eighty-two: eighty-three: eighty-four: eighty-five: eighty-six: eighty-seven: eighty-eight: eighty-nine: ninety: ninety-one: ninety-two: ninety-three: ninety-four: ninety-five: ninety-six: ninety-seven: ninety-eight: ninety-nine: one-hundred: one-hundred-one: one-hundred-two: one-hundred-three:))))
        (when (mouse:? data)
            (let* ((mouse (mouse: data))
                   (color (color: mouse)))
                (on-device "mouse" show: color)))
        (when (wheel:? data)
            (let* ((wheel (wheel: data))
                   (color (color: wheel)))
                (on-device "mouse" show-on-zone: color wheel:)))
        (when (logo:? data)
            (let* ((logo (logo: data))
                   (color (color: logo)))
                (on-device "mouse" show-on-zone: color logo:)
                (on-device "mouse" show-on-zone: color base:)
                (on-device "headset" show-on-zone: color earcups:)))
    )
)
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-1-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-2-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-3-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-4-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-5-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-8-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-12-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-17-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-24-zone")
(add-event-zone-use-with-specifier "AURORA" "all" "rgb-103-zone")
(add-event-per-key-zone-use "AURORA" "all")