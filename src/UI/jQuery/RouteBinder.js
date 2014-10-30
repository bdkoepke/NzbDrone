'use strict';
define(
    [
        'backbone',
        'jquery',
        'System/StatusModel'
    ], function (Backbone, $, StatusModel) {
        //This module will automatically route all relative links through backbone router rather than
        //causing links to reload pages.

        var routeBinder = {

            bind: function () {
                var self = this;
                $(document).on('click', 'a[href]', function (event) {
                    self._handleClick(event);
                });
            },

            _handleClick: function (event) {
                var $target = $(event.target);

                //check if tab nav
                if ($target.parents('.nav-tabs').length) {
                    return;
                }

                if ($target.hasClass('no-router')) {
                    return;
                }

                var href = event.target.getAttribute('href');

                if (!href && $target.closest('a') && $target.closest('a')[0]) {

                    var linkElement = $target.closest('a')[0];

                    if ($(linkElement).hasClass('no-router')) {
                        return;
                    }

                    href = linkElement.getAttribute('href');
                }

                event.preventDefault();

                if (!href) {
                    throw 'couldn\'t find route target';
                }

                if (!href.startsWith('http')) {
                    if (event.ctrlKey) {
                        window.open(href, '_blank');
                    }

                    else {
                        var relativeHref = href.replace(StatusModel.get('urlBase'), '');

                        Backbone.history.navigate(relativeHref, { trigger: true });
                    }
                }
                else if (href.contains('#')) {
                    //Open in new tab without dereferer (since it doesn't support fragments)
                    window.open(href, '_blank');
                }
                else {
                    //Open in new tab
                    window.open('http://www.dereferer.org/?' + encodeURI(href), '_blank');
                }
            }
        };

        return routeBinder;
    });
