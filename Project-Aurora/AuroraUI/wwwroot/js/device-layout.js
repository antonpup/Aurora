(function () {
    window.deviceLayout = {

        /** Initialises the JS logic for the global device layout.
         * @param {HTMLElement} element The target element to initialise the device layout on.
         * @param {{invokeMethodAsync(name:string, ...params):void}} deviceLayout A .NET reference object for the layout controller. */
        init(element, deviceLayout) {
            deviceLayout.invokeMethodAsync("X", "Client message");
        }
    };
})();