(function() {
    window.auroraUi = window.auroraUi || {};
    window.auroraUi.colorPicker = {

        /** @type {Map<HTMLElement, any>} */
        elementMap: new Map(),

		/** Initialises the pickr color picker on the target element.
		 * @param {HTMLElement} trigger The element that will be used as the trigger for the picker.
		 * @param {{invokeMethodAsync(remoteMethodName:string, ...params):void}} colorPicker The .NET object reference to the Blazor component. */
        init(trigger, colorPicker) {
			let buttonBlock = trigger.querySelector('.color-block');
			let buttonLabel = trigger.querySelector('.color-name');

			let pickr = new Pickr({
				el: trigger,
				useAsButton: true,
				theme: 'classic',
				components: {
					palette: true,
					preview: true,
					opacity: true,
					hue: true,
					interaction: {
						hex: true,
						rgba: true,
						hsla: true,
						hsva: true,
						input: true,
						clear: true,
						save: true
					}
				}

			}).on('changestop', i => { // was originally change but was making too many requests to the server, causing it to lag. Maybe could be replaced with change again if the event is debounced?
				let c = i.getColor();
				let cArr = c == null ? [0, 0, 0, 0] : c.toRGBA();
				// The values may be decimal, but C# requires bytes, so we need to round the values first. Also alpha is 0-1 instead of 0-255
				colorPicker.invokeMethodAsync("UpdateColor", Math.round(cArr[0]), Math.round(cArr[1]), Math.round(cArr[2]), Math.round(cArr[3] * 255));
				buttonBlock.style.background = c.toRGBA().toString(0);
				buttonLabel.textContent = c.toHEXA().toString();
			});

			window.auroraUi.colorPicker.elementMap.set(trigger, pickr);
        },

		/** Disposes of and destroys DOM elements that were created as part of the pickr for this element.
		 * @param {HTMLElement} element The element that was used as the trigger for the picker. */
        dispose(element) {
			let m = window.auroraUi.colorPicker.elementMap;
			if (m.has(element)) {
				m.get(element).destroyAndRemove();
				m.delete(element);
			}
        }
    }
})();