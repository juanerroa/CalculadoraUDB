$('[data-toggle="minimize"]').on("click", function () {
    $('body').toggleClass('sidebar-icon-only');
});

function closeSnoAlertBox() {
    window.setTimeout(function () {
        $("#snoAlertBox").fadeOut(300)
    }, 3000);
}

function MostrarAlerta(clase, texto) {
    $('#snoAlertBox').removeClass().addClass(clase);
    $('#snoAlertBox').html(texto);
    $("#snoAlertBox").fadeIn();
    closeSnoAlertBox();
}

function togglePerfil() {
    if ($('#MiPerfil').css('left') == '0px') {
        $('#MiPerfil').animate({ left: '-100%' }, 500);
        setTimeout(function () { $('#MiPerfil').hide(); }, 500);
    } else {
        $('#MiPerfil').show();
        $('#MiPerfil').animate({ left: 0 }, 500);
    }
}

$('#closePerfil').click(function () {
    togglePerfil();
});

$('#footer').click(function () {
    togglePerfil();
});