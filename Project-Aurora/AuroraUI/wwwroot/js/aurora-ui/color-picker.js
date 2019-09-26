(function() {

	/** @type {Map<HTMLElement, any>} */
	const elementMap = new Map()

    window.auroraUi = window.auroraUi || {};
    window.auroraUi.colorPicker = {

		/** Initialises the pickr color picker on the target element.
		 * @param {HTMLElement} trigger The element that will be used as the trigger for the picker.
		 * @param {{invokeMethodAsync(remoteMethodName:string, ...params):void}} colorPicker The .NET object reference to the Blazor component.
		 * @param {string} initial The initial value of the color picker. */
        init(trigger, colorPicker, initial) {
			let buttonBlock = trigger.querySelector('.color-block');
			let buttonLabel = trigger.querySelector('.color-name');

			let pickr = new Pickr({
				el: trigger,
				default: initial || '#000000',
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

			buttonBlock.style.background = buttonLabel.textContent = initial || '#000000';

			elementMap.set(trigger, pickr);
		},
		
		/** Updates the color of a color picker control.
		 * @param {HTMLElement} element The picker trigger element.
		 * @param {string} color The new color for the control. */
		setColor(element, color) {
			if (elementMap.has(element)) {
				elementMap.get(element).setColor(color);
				let c = elementMap.get(element).getColor();
				element.querySelector('.color-block').style.background = c.toRGBA().toString(0);
				element.querySelector('.color-name').textContent = c.toHEXA().toString();
			}
		},

		/** Disposes of and destroys DOM elements that were created as part of the pickr for this element.
		 * @param {HTMLElement} element The element that was used as the trigger for the picker. */
        dispose(element) {
			if (elementMap.has(element)) {
				elementMap.get(element).destroyAndRemove();
				elementMap.delete(element);
			}
        }
    }
})();