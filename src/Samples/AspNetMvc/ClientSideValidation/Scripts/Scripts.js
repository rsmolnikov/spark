$(function () {
    // Prevent dynamic validation.  Only allow eager validation.
    $.validator.setDefaults({
        onfocusout: function (element) {
        },
        onkeyup: function (element) {
        }
    });

//    $.validator.setDefaults({
//        showErrors: function (errorMap, errorList) {

//            
//        }
//    });
   
    $('form').clientValidation();
});