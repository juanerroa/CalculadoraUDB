
(function ($) {
    "use strict";
    /*==================================================================
    [ Validate ]*/
    var input = $('.validate-input .input100');

    $("form").submit(function (e) {
        e.preventDefault();

        var check = true;

        for (var i = 0; i < input.length; i++) {
            if (validate(input[i]) == false) {
                showValidate(input[i]);
                check = false;
            }
        }

        if (check == true)
            Login();
    });

    function Login() {
        $('#submitBtn').prop('disabled', true);
        $('#submitBtn').html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Conectando...');

        let carnet = $('#carnet').val();
        let password = $('#password').val()

        $.ajax({
            url: '/Login/TryToLogin',
            type: 'post',
            dataType: 'json',
            data: { carnet, password },
            success: function (resp) {
                if (resp == 'logged') {
                    window.location.replace("/Expediente");
                }
                else {
                    alertErrorLogin();
                }
            },
            error: function (resp) {
                alertErrorLogin();
            }
        });
    }

    function alertErrorLogin() {
        $("#snoAlertBox").fadeIn();
        $('#submitBtn').prop('disabled', false);
        $('#submitBtn').html('INICIAR SESION');
        closeSnoAlertBox();
    }

    $('.validate-form .input100').each(function(){
        $(this).focus(function(){
           hideValidate(this);
        });
    });

    function validate (input) {
        if($(input).attr('type') == 'email' || $(input).attr('name') == 'email') {
            if ($(input).val().trim().match(/^[a-zA-Z]{2}\d{6}$/) == null) {
                return false;
            }
        }
        else {
            if($(input).val().trim() == ''){
                return false;
            }
        }
    }

    function showValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).addClass('alert-validate');
    }

    function hideValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).removeClass('alert-validate');
    }

})(jQuery);

function ToggleProblemasModal() {
    $('#problemasModal').modal('toggle');
}

function closeSnoAlertBox() {
    window.setTimeout(function () {
        $("#snoAlertBox").fadeOut(300)
    }, 3000);
}

function togglePerfil() {
    if ($('#MiPerfil').css('left') == '0px') {
        $('#MiPerfil').animate({ left: '-100%' }, 500);
        setTimeout(function () {$('#MiPerfil').hide();}, 500);
    } else {
        $('#MiPerfil').show();
        $('#MiPerfil').animate({ left: 0 }, 500);
    }
}

$('#closePerfil').click(function () {
    togglePerfil();
});

$('footer').click(function () {
    togglePerfil();
});