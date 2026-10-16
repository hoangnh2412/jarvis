window.workflowPreview = window.workflowPreview || {
    attachBackgroundClick: function (element, dotNetReference) {
        if (!element) {
            return;
        }

        this.detachBackgroundClick(element);

        const handler = function (event) {
            const target = event.target instanceof Element
                ? event.target
                : null;

            if (target && target.closest('.x6-node')) {
                return;
            }

            const cell = target?.closest('[data-cell-id]');
            if (cell && !cell.classList.contains('x6-edge')) {
                return;
            }

            dotNetReference.invokeMethodAsync('ClearSelectedActivityFromBackground');
        };

        element.addEventListener('click', handler);
        element.__workflowPreviewBackgroundClickHandler = handler;
    },

    detachBackgroundClick: function (element) {
        const handler = element?.__workflowPreviewBackgroundClickHandler;
        if (!handler) {
            return;
        }

        element.removeEventListener('click', handler);
        delete element.__workflowPreviewBackgroundClickHandler;
    }
};
