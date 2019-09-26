(function () {

	/** @type {Map<HTMLElement, object>} */
	let elementDict = new Map();

    window.auroraUi = window.auroraUi || {};
    window.auroraUi.scrollbar = {

		init(element) {
			let scrollbar = new PerfectScrollbar(element);
			elementDict.set(element, scrollbar);
		},

		update(element) {
			if (elementDict.has(element))
				elementDict.get(element).update();
		},

		dispose(element) {
			if (elementDict.has(element)) {
				elementDict.get(element).destroy();
				elementDict.delete(element);
			}
		}
	}
})();