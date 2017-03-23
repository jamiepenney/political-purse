window.Data = require('./data');

$(() => {
    $('.js-marketing-notify-form').on('submit', function() {
        // check email address field
        const emailField = $(this).find('[name=email]');
        const emailInputGroup = emailField.parent();
        const emailFieldFeedback = emailInputGroup.find('.form-control-feedback');
        const val = emailField.val();

        if(!val || val.indexOf('@') == -1) {
            emailInputGroup.addClass('has-danger');
            emailFieldFeedback.removeClass('hide').text('You must enter a vaild email address');
        } else {
            var buttonEl = $(this).find('.js-marketing-notify-submit');
            var originalButtonText = buttonEl.html();
            buttonEl.attr('disabled', 'disabled').html('<i class="fa fa-spin fa-spinner"></i>');

            $.post('/notifications/signup', {
                email: emailField.val()
            }).fail(() => {
                emailInputGroup.addClass('has-danger');
                emailFieldFeedback
                    .removeClass('hide')
                    .text('We couldn\'t sign you up - try again later?');
            }).done((result, statusText, jqxhr) => {
                emailFieldFeedback.removeClass('hide')
                    .text('All signed up - you should receive an email shortly!')
                emailInputGroup
                    .removeClass('has-danger')
                    .addClass('has-success');
                emailField.addClass('form-control-success');
            }).always(() => {
                buttonEl.removeAttr('disabled').html(originalButtonText);
            });
        }

        return false;
    });
});