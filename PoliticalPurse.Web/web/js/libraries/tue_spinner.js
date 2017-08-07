/**
 *  https://github.com/wasinger/tue_spinner
 *  Centered "Loading..." Animation (Spinner) for jQuery and Bootstrap4
 *
 *  Requires Modal component from Bootstrap4.
 *
 *  Can be used globally on a page or on any block element. When used globally the whole browser window
 *  will be darkended and the spinner appears in the center (within a Bootstrap modal with static backdrop),
 *  and no user interaction is possible any more until the spinner is removed.
 *
 *  When used on a block element, only this element will be darkened.
 *
 *  Global usage:
 *
 *  $.spinner('show');
 *  $.spinner('hide');
 *
 *  Usage on a block element:
 *
 *  $('#mydiv').spinner('show');
 *  $('#mydiv').spinner('hide');
 *
 *
 */

;(function($) {
    var dataname = 'tuespinner';

    var Spinner = function(element) {
        this.element = element;
        this.$element = $(element);
        /**
         * Spinner HTML Code based on "Fading circle" animation from SpinKit by Tobias Ahlin
         * https://github.com/tobiasahlin/SpinKit/blob/master/scss/spinners/10-fading-circle.scss
         * @license MIT
         */
        this.spinner = $('<div class="spinner" role="progressbar"><div class="sk-circle1 sk-circle"></div><div class="sk-circle2 sk-circle"></div><div class="sk-circle3 sk-circle"></div><div class="sk-circle4 sk-circle"></div><div class="sk-circle5 sk-circle"></div><div class="sk-circle6 sk-circle"></div><div class="sk-circle7 sk-circle"></div><div class="sk-circle8 sk-circle"></div><div class="sk-circle9 sk-circle"></div><div class="sk-circle10 sk-circle"></div><div class="sk-circle11 sk-circle"></div><div class="sk-circle12 sk-circle"></div></div>');
        this._init();
    };

    Spinner.prototype = {
        oldheight: undefined,
        oldposition: undefined,
        _init: function() {
            var size;
            if (this.element.tagName == 'BODY') {
                this.modal = $('<div class="modal spinner-modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" aria-hidden="true"></div>').appendTo(this.$element);
                size = Math.min(Math.min(window.innerWidth, window.innerHeight) * .2, 80);
                this.modal.append(this.spinner);
            } else {
                var el_h = this.$element.height();
                if (el_h < 25) {
                    size = 20;
                    this.oldheight = this.$element.css('height');
                    this.$element.height(25);
                } else {
                    size = Math.min(el_h * .8, 60);
                }
                this.spinner.appendTo(this.$element).hide().addClass('fade');
                this.spinner_backdrop = $('<div class="spinner-backdrop fade">');
                this.spinner_backdrop.appendTo(this.$element).hide();
            }
            this.spinner.css({
                width: size + 'px',
                height: size + 'px',
                position: 'absolute',
                top: '50%',
                left: '50%',
                marginTop: '-' + (size / 2) + 'px',
                marginLeft: '-' + (size / 2) + 'px'
            });
        },

        show: function () {
            if (!$.contains(this.$element, this.spinner)) {
                this._init();
            }
            if (this.modal) {
                this.modal.modal('show');
            } else {
                var position = this.$element.css('position');
                if (!(position == 'absolute' || position == 'relative' )) {
                    this.oldposition = position;
                    this.$element.css({
                        'position': 'relative',
                        'top': 0,
                        'left': 0
                    });
                }
                this.spinner_backdrop.show().addClass('in');
                this.spinner.show().addClass('in');
            }

        },
        hide: function () {
            if (this.modal) {
                this.modal.modal('hide');
            } else {
                this.spinner.removeClass('in');
                this.spinner_backdrop.removeClass('in');
                var spinner = this;
                this.spinner.one('bsTransitionEnd',  function() {
                    $(this).hide();
                    if (this.oldposition != undefined) {
                        $(spinner.element).css('position', this.oldposition);
                    }
                    if (this.oldheight != undefined) {
                        $(spinner.element).height('').css('height', this.oldheight);
                    }
                }).emulateTransitionEnd(300); // $.fn.emulateTransitionEnd wird von bootstrap.js definiert
                this.spinner_backdrop.one('bsTransitionEnd',  function() {
                    $(this).hide();
                }).emulateTransitionEnd(300);
            }
        }
    };

    $.fn.tue_spinner = function(method) {
        var instance;
        return this.each(function () {
            instance = $.data(this, dataname);
            if (!instance || !(instance instanceof Spinner)) {
                instance = new Spinner(this);
                $.data(this, dataname, instance);
            }
            if (typeof method === 'string'
                && method.charAt(0) !== '_'
                && typeof Spinner.prototype[method] === 'function'
            ) {
                instance[method].apply(instance);
            }
        });
    };

    $.tue_spinner = function(method) {
        return $(document.body).tue_spinner(method);
    }

})(jQuery);