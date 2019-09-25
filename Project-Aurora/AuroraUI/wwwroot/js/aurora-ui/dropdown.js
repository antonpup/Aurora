(function () {

    window.auroraUi = window.auroraUi || {};
    window.auroraUi.dropdown = {

		/** @type {Map<HTMLElement, any>} */
		popperMap: new Map(),

		/** Initialises the code for the dropdown
		 * @param {HTMLElement} trigger The element that will trigger the popover.
		 * @param {HTMLElement} popElement The element that will be popped.
		 * @param {{invokeMethodAsync(remoteMethodName:string, ...params):void}} dotnet
		 * @param {string} placement The placement of the popper. */
		init(trigger, popElement, dotnet, placement) {
			let popper = new Popper(trigger, popElement, {
				placement
			});
			window.auroraUi.dropdown.popperMap.set(trigger, popper);

			let isOpen = false;

			trigger.addEventListener('click', () => {
				if (isOpen) close();
				else open();
			});

			function open() {
				popElement.classList.add('show');
				document.addEventListener('mousedown', documentMouseDownHandler); // Trigger the dropdown to close when the mouse goes down outside the popover
				document.addEventListener('click', documentClickHandler); // But only trigger a dismiss on a click (so buttons etc still recieve it)
				isOpen = true;
			}

			function close() {
				popElement.classList.remove('show');
				document.removeEventListener('mousedown', documentMouseDownHandler);
				document.removeEventListener('click', documentClickHandler);
				isOpen = false;
			}

			function documentMouseDownHandler(e) {
				// Outside click if the element that was clicked on is not inside the popper or the trigger
				let clickOutside = e.target != trigger && e.target != popElement && !trigger.contains(e.target) && !popElement.contains(e.target);
				if (clickOutside || isDismiss) close();
			}

			function documentClickHandler(e) {
				// Check if the element clicked on is inside an element marked with 'data-dropdown-dismiss'
				let closestDismiss = e.target.closest('[data-dropdown-dismiss]');
				let isDismiss = closestDismiss != null && popElement.contains(closestDismiss);
				if (isDismiss) close();
			}
		},

		dispose(trigger) {
			let m = window.auroraUi.dropdown.popperMap;
			if (m.has(trigger)) {
				m.get(trigger).dispose();
				m.delete(trigger);
			}
		}
	};
})();