(function () {
    'use strict';

    function metaTagsService() {
        var service = this;
        service.setDefaultTags = setDefaultTags;
        service.setTags = setTags;
        var defaultTags = {};
        var tagElements = [];
        function setDefaultTags(tags) {
            angular.copy(tags, defaultTags);
            setTags({});
        }
        function setTags(tags) {
            clearTags();
            mergeDefaultTags(tags);
            angular.forEach(tags, function (content, name) {
                var tagElement = getTagElement(content, name);
                document.head.appendChild(tagElement);
                tagElements.push(tagElement);
            });
        }
        function mergeDefaultTags(tags) {
            angular.forEach(defaultTags, function (defaultTagContent, defaultTagName) {
                if (!tags[defaultTagName]) {
                    tags[defaultTagName] = defaultTagContent;
                } else if (defaultTagName === 'title') {
                    tags['title'] += ' - ' + defaultTagContent;
                }
            });
            return tags;
        }
        function getTagElement(content, name) {
            if (name == 'title') {
                // Special provision for the title element
                var title = document.createElement('title');
                title.textContent = content;
                return title;
            } else {
                // Opengraph uses [property], but everything else uses [name]
                var nameAttr = (name.indexOf('og:') === 0) ? 'property' : 'name';
                var meta = document.createElement('meta');
                meta.setAttribute(nameAttr, name);
                meta.setAttribute('content', content);
                return meta;
            }
        }
        function clearTags() {
            angular.forEach(tagElements, function (tagElement) {
                document.head.removeChild(tagElement);
            });
            tagElements.length = 0;
        }
    }

    angular
        .module('jarvis')
        .service('metaTagsService', metaTagsService);
}());