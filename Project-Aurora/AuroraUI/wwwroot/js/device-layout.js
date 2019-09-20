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
			let devId, devEl, devGhost;
			let devX, devY;
            let mouseLastX, mouseLastY;

            svg.addEventListener('pointerdown', e => {
				e.preventDefault();
				svg.setPointerCapture(e.pointerId);

                if (e.isPrimary && svg.dataset["editmode"] == "True" && e.target != svg) {
					devEl = e.target;
					while (isNaN(devId = +devEl.dataset["deviceId"]) && devEl != svg) {
						devEl = devEl.parentElement;
					}
					if (!isNaN(devId)) {
						let match = new RegExp(/translate\((\-?\d+) (\-?\d+)\)/).exec(devEl.getAttribute('transform'));
						devX = +match[1]; devY = +match[2];
						devGhost = devEl.cloneNode(true);
						devGhost.style.pointerEvents = "none";
						devGhost.style.opacity = 0.4;
						devGhost.zIndex = 1;
						devEl.parentElement.appendChild(devGhost);
						mode = "move";
						document.body.style.cursor = "move";
					}
						
                } else if (e.isPrimary) {
					mode = "pan";
					document.body.style.cursor = "move";
                }
            });

            // We listen on the document (instead of the SVG element) for mousemove events so that dragging doesn't abruptly stop if the user leaves the SVG region
            svg.addEventListener('pointermove', e => {
				let deltaX = mouseLastX - e.clientX, deltaY = mouseLastY - e.clientY;

				if (mode == "move") {
					//console.log(devX, devY, devG);
					devX -= deltaX / zoom;
					devY -= deltaY / zoom;
					devGhost.setAttribute('transform', `translate(${devX} ${devY})`);

				} else if (mode == "pan") {
                    panX -= deltaX;
                    panY -= deltaY;
                    updateTransform();
                }
                mouseLastX = e.clientX;
				mouseLastY = e.clientY;
				
				let [x, y] = mouseEventToSvgCoords(e);
				document.getElementById("test").setAttribute("cx", x);
				document.getElementById("test").setAttribute("cy", y);
            });

            // Listen for mouse up events anywhere on the page, not just if the mouse is over the SVG element
            svg.addEventListener('pointerup', e => {
				if (mode == "move") {
					deviceLayout.invokeMethodAsync("SetDevicePosition", devId, devX, devY);
					//devEl.setAttribute()
				}
				if (devGhost) {
					devGhost.remove();
					devGhost = null;
				}

				svg.releasePointerCapture(e.pointerId);
				mode = "none";
				document.body.style.cursor = null;
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
                panX += (relX * zoomRatio) - relX;
                panY += (relY * zoomRatio) - relY;
                
                updateTransform();
            });


            // Update the SVG's transform
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
        }
    };
})();