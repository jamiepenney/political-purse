module.exports = {
    start: function(element) {
        var $el = $(element);
        $el.data('old-contents', $el.html());
        $el.html('<div class="loader-container"><div class="loader">' + '<span class="loader-block"></span>'.repeat(9) + '</div></div>');
    },

    end: function(element){
        var $el = $(element);
        $el.html($el.data('old-contents'));
    }
};