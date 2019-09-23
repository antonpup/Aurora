(function () {
    // Some config constants
    const MIN_ZOOM = 0.4;
    const MAX_ZOOM = 3;
    const ZOOM_DELTA = -0.01;

    window.auroraUi = window.auroraUi || {};
    window.auroraUi.deviceLayout = {

        /** Initialises the JS logic for the global device layout.
         * @param {SVGSVGElement} svg The target element to initialise the device layout on.
         * @param {{invokeMethodAsync(remoteMethodName:string, ...params):void}} deviceLayout A .NET reference object for the layout controller. */
        init(svg, deviceLayout) {

            let zoom = 1;
            let panX = 0, panY = 0;

            /** @type {"none"|"pan"|"move"} */
            let mode = "none";
            let devId = -1;
            /** @type {SVGElement} */
            let deviceElement;
            /** @type {SVGGElement} */
            let deviceGhost;
			let devX = 0, devY = 0;
            let mouseLastX = 0, mouseLastY = 0;


            svg.addEventListener('pointerdown', e => {
				e.preventDefault();
				svg.setPointerCapture(e.pointerId); // Capture the pointermove/pointerup events even when the user moves the mouse off the element or even the browser

                // If the canvas is in edit mode and the mousedown target is NOT the root SVG element, the user must have clicked on a device, so start moving that device
                if (e.isPrimary && svg.dataset["editmode"] == "True" && e.target != svg) {
                    deviceElement = e.target;
                    // The e.target may not be the SVG G element that represents the device, it may be a descendant element of this (e.g. a key, light, image, etc.)
                    // So we keep searching up the DOM tree from the e.target element until we find one that is a SVG G element representing a device (it will have a data-device-id attribute)
                    while (isNaN(devId = +deviceElement.dataset["deviceId"]) && deviceElement != svg)
                        deviceElement = deviceElement.parentElement;

                    // Once the while loop has exited, check we have a deviceId. If this isn't the case, the e.target was an element that was not a child of a device.
                    // The device id should always have a value since only devices are ever in the SVG, but this will future proof it.
                    if (!isNaN(devId)) {
                        devX = +deviceElement.dataset['deviceX']; devY = +deviceElement.dataset['deviceY'];
                        deviceGhost = createGhost(deviceElement);
                        mode = "move";
                        svg.style.cursor = "move";
                    }
                }

                // If the user didn't click a device group element (or they clicked an invalid item), then start panning.
                if (e.isPrimary && mode == "none") {
                    mode = "pan";
                    svg.style.cursor = "grabbing";
                }
            });


            svg.addEventListener('pointermove', e => {
                // Calculate the delta distance that the mouse has moved since the last pointermove event.
				let deltaX = mouseLastX - e.clientX, deltaY = mouseLastY - e.clientY;

				if (mode == "move") {
                    // When moving a device, update the device X and Y position based on the amount the mouse has moved
                    // Must take into account the zoom level otherwise the device won't move enough or will move too much when zoomed in/out
					devX -= deltaX / zoom;
					devY -= deltaY / zoom;
                    // Update the ghost's position
					deviceGhost.setAttribute('transform', `translate(${devX} ${devY})`);

				} else if (mode == "pan") {
                    // When panning, we simply update the panX and panY values. No need to take zoom into account here because the
                    // main SVG container element's transform is applied in the order: translate first, scale second.
                    panX -= deltaX;
                    panY -= deltaY;
                    updateTransform();
                }

                mouseLastX = e.clientX;
				mouseLastY = e.clientY;
            });


            svg.addEventListener('pointerup', e => {
                // On mouse release, if we're moving an element, send the new updated device position to the Blazor server.
                if (mode == "move") {
                    deviceLayout.invokeMethodAsync("SetDevicePosition", devId, devX, devY);
                    deviceElement.setAttribute('transform', `translate(${devX} ${devY})`); // Update the device position immediately otherwise it appears to 'flash' for a few ms as the Blazor server updates it
                }

                // If there is a device ghost element, remove it
				if (deviceGhost) {
					deviceGhost.remove();
					deviceGhost = null;
				}

				svg.releasePointerCapture(e.pointerId); // Release our mouse capture on the pointer
				mode = "none";
				svg.style.cursor = null;
            });


            // Scroll wheel handler
            svg.addEventListener('wheel', e => {
                // Get the mouse position, relative to the SVG canvas.
				// This is the point that should remain stationary, relative to the viewport
				let [relX, relY] = mouseEventToSvgCoords(e);

                // Calculate new zoom level and a ratio between new and old zoom
                let oldZoom = zoom;
                zoom = Math.min(Math.max(zoom + e.deltaY * ZOOM_DELTA, MIN_ZOOM), MAX_ZOOM);
				let zoomRatio = oldZoom / zoom;

                // Pan the canvas slightly so that the point under the mouse doesn't appear to move relative to the viewport
                // TODO: still not exactly how I would like it
                panX += (relX * zoomRatio) - relX;
                panY += (relY * zoomRatio) - relY;
                
                updateTransform();
            });


            /** Update the SVG element's transform attribute to reflect the current pan and zoom amounts. */
            function updateTransform() {
				svg.firstElementChild.setAttribute('transform', `translate(${panX}, ${panY}) scale(${zoom})`);
			}			

			/** Converts the position of the mouse during the given event relative to the SVG canvas.
			 * @param {MouseEvent} e The source mouse event. */
			function mouseEventToSvgCoords(e) {
				// Cannot use e.offsetX/e.offsetY because this is relative event.target, which, if the mouse is over a device will be that device, not the SVG
				let svgPos = svg.getBoundingClientRect();
				return elementToSvgCoords(e.clientX - svgPos.left, e.clientY - svgPos.top);
			}

            /** Converts a pair X and Y coordinates that are relative to the target SVG element into coordinates that are relative to the SVG items inside the SVG.
             * @param {number} elementX The X coordinate relative to the target SVG element.
             * @param {number} elementY The Y coordinate relative to the target SVG element.
             * @returns {[number,number]} The [X, Y] coordinates relative to the SVG canvas, taking into account pan and zoom level. */
            function elementToSvgCoords(elementX, elementY) {
                return [
                    (elementX - panX) / zoom,
                    (elementY - panY) / zoom
                ];
            }

            /** Creates a 'ghost' clone of the target element. The ghost is identical to the target node but does not receive mouse events and is semi-transparent.
             * Will also add the ghost to the same container as the target element.
             * @param {HTMLElement} el The target element to clone. */
            function createGhost(el) {
                let ghost = el.cloneNode(true);
                ghost.dataset['ghost'] = "true";
                ghost.style.pointerEvents = "none"; // ignore mouse
                ghost.style.opacity = 0.4;
                ghost.zIndex = 1; // appear on top of other elements
                el.parentElement.appendChild(ghost);
                return ghost;
            }
        }
    };
})();