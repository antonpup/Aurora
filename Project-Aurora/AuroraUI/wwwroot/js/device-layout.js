(function () {
    // Some config constants
    const MIN_ZOOM = 0.4;
    const MAX_ZOOM = 3;
    const ZOOM_DELTA = 0.01;

    window.auroraUi = window.auroraUi || {};
    window.auroraUi.deviceLayout = {

        /** Initialises the JS logic for the global device layout.
         * @param {SVGSVGElement} element The target element to initialise the device layout on.
         * @param {{invokeMethodAsync(remoteMethodName:string, ...params):void}} deviceLayout A .NET reference object for the layout controller. */
        init(element, deviceLayout) {
//            deviceLayout.invokeMethodAsync("X", "Client message");

            let zoom = 1;
            let panX = 0, panY = 0;

            /** @type {"none"|"pan"|"move"} */
            let mode = "none";
            let mouseLastX, mouseLastY;

            element.addEventListener('mousedown', e => {
                e.preventDefault();
                if (e.button == 0 && element.dataset["editmode"] == "True" && e.target != element) {
                    // TODO: Drag some particular device
                    // Will need to store which device it is
                    mode = "move";
                } else if (e.button == 0) {
                    mode = "pan";
                }
                mouseLastX = e.clientX;
                mouseLastY = e.clientY;
            });

            // Listen on the document for mousemove events so that dragging doesn't abruptly stop if the user leaves the SVG region
            document.addEventListener('mousemove', e => {
                if (mode == "pan") {
                    panX -= mouseLastX - e.clientX;
                    panY -= mouseLastY - e.clientY;
                    updateTransform();
                }
                mouseLastX = e.clientX;
                mouseLastY = e.clientY;
            });

            // Listen for mouse up events anywhere on the page
            document.addEventListener('mouseup', e => {
                mode = "none";
            });

            // Scroll wheel handler
            element.addEventListener('wheel', e => {
                zoom = Math.min(Math.max(zoom + e.deltaY * ZOOM_DELTA, MIN_ZOOM), MAX_ZOOM);
                // TODO: Make it zoom in at the mouse's position, rather than the SVG's origin
                updateTransform();
            });

            // Update the SVG's transform
            function updateTransform() {
                element.firstElementChild.setAttribute('transform', `translate(${panX}, ${panY}) scale(${zoom})`);
            }
        }
    };
})();