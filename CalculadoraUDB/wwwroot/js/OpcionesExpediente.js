class MateriaExpediente {
    constructor(Anio, Ciclo, Codigo, Asignatura, Matricula, Nota, Resultado) {
        this.Anio = Anio;
        this.Ciclo = Ciclo;
        this.Codigo = Codigo;
        this.Asignatura = Asignatura;
        this.Matricula = Matricula;
        this.Nota = Nota;
        this.Resultado = Resultado;
    }
}

function GetMateriaObject() {
    let anio = $('#formOptionMateria #Anio').val();
    let ciclo = $('#formOptionMateria #Ciclo').val();
    let matricula = $('#formOptionMateria #Matricula').val();
    let codigo = $('#formOptionMateria #Codigo').val();
    let asignatura = $('#formOptionMateria #Asignatura').val();
    let nota = $('#formOptionMateria #Nota').val();
    let resultado = $('#formOptionMateria #Resultado').val();
    let materiaObject = new MateriaExpediente(anio, ciclo, codigo, asignatura, matricula, nota, resultado);
    return materiaObject;
}

$(document).ready(function () {
    UpdateResultados();
});

function UpdateResultados() {
    $.ajax({
        url: "/Expediente/GetJsonResultados",
        method: "post",
        dataType: "json",
        success: function (resultado) {
            shakeResult($('#cum'), resultado.Cum, 500);
            shakeResult($('#promedio'), resultado.Promedio, 1000);
            shakeResult($('#avance'), resultado.Avance + '%', 1500);
            shakeResult($('#uvGanadas'), resultado.UvGanadas, 2000);
        }
    });
}

function materiaRowToModal(row) {
    eraseInputsFromFormModel();
    $('#formOptionMateria #Codigo').html('');
    let tr = row.parent().parent();
    let rowClicked = [];

    td = tr.find('td').each(function (index, value) {
        if (index < 7)
            rowClicked.push($(this).html());
    });

    $('#formOptionMateria #Anio').val(rowClicked[0]);
    $('#formOptionMateria #Ciclo').val(rowClicked[1]);
    $('#formOptionMateria #Matricula').val(rowClicked[4]);
    $('#formOptionMateria #Codigo').append('<option value="' + rowClicked[2] + '">' + rowClicked[3] + '</option>');
    $('#formOptionMateria #Nota').val(rowClicked[5]);
    $('#formOptionMateria #Resultado').val(rowClicked[6]);
    if (rowClicked[6] == 'Retirada' || rowClicked[6] == 'Ret. Total')
        $('#formOptionMateria #Retirada').prop('checked', false);
    else
        $('#formOptionMateria #Retirada').prop('checked', true);
}

$('#addMateriaBtn').click(function () {
    eraseInputsFromFormModel();
    $('#MateriaModal #header').removeClass().addClass('modal-header bg-success');
    $('#MateriaModal #title').html('Agregar Materia');
    $('#MateriaModal #submit').html('Agregar Materia');
    $('#MateriaModal #submit').removeClass().addClass('btn btn-info');
    $('#formOptionMateria').attr('action', 'Agregar');

    $.ajax({
        url: '/Expediente/GetJsonMateriasFaltantes',
        type: 'post',
        dataType: 'json',
        success: function (materias) {
            $('#formOptionMateria #Codigo').html('');
            $.each(materias, function (index, value) {
                $('#formOptionMateria #Codigo').append('<option value="' + value.Codigo + '">' + value.Nombre + '</option>');
            });
            $('#formOptionMateria #Codigo').change();
            disableInputsFromFormModel(false);
            $('#MateriaModal').modal('show');
        }
    });
});

var materiaBeforeUpdate;
$('#expedienteTbl').on('click', '#editMateriaBtn', function () {
    materiaRowToModal($(this));
    $('#formOptionMateria #Retirada').click();
    $('#MateriaModal #header').removeClass().addClass('modal-header bg-warning');
    $('#MateriaModal #title').html('Editar Materia');
    $('#MateriaModal #submit').html('Editar Materia');
    $('#MateriaModal #submit').removeClass().addClass('btn btn-info');
    $('#formOptionMateria').attr('action', 'Editar');
    $('#formOptionMateria #Codigo').change();
    materiaBeforeUpdate = GetMateriaObject();
    disableInputsFromFormModel(false);
    $('#MateriaModal').modal('show');

});

$('#expedienteTbl').on('click', '#deleteMateriaBtn', function () {
    materiaRowToModal($(this));
    $('#formOptionMateria #Retirada').click();
    $('#MateriaModal #header').removeClass().addClass('modal-header bg-danger');
    $('#MateriaModal #title').html('Eliminar Materia');
    $('#MateriaModal #submit').html('Eliminar Materia');
    $('#MateriaModal #submit').removeClass().addClass('btn btn-info');
    $('#formOptionMateria').attr('action', 'Eliminar');
    $('#formOptionMateria #Codigo').change();
    disableInputsFromFormModel(true);
    $('#MateriaModal').modal('show');
});

$('#formOptionMateria').submit(function (e) {
    $('#submit').prop('disabled', true);
    e.preventDefault();
    let materia = GetMateriaObject();
    let urlToOption;
    let action = $('#formOptionMateria').attr('action');
    if (action == 'Agregar')
        urlToOption = "/Expediente/AgregarMateria";
    else if (action == 'Editar')
        urlToOption = "/Expediente/EditarMateria";
    else if (action == 'Eliminar')
        urlToOption = "/Expediente/EliminarMateria";

    $.ajax({
        url: urlToOption,
        type: 'post',
        dataType: 'json',
        data: { materia, materiaBeforeUpdate },
        success: function (resultado) {
            expedienteTbl.ajax.reload(null, false);
            $('#MateriaModal').modal('hide');
            $('#submit').prop('disabled', false);
            MostrarAlerta(resultado.clase, resultado.texto)
            UpdateResultados();
        }
    });

    return false;
});

function actualizarResultadoModal(nota) {
    let resultado = $('#formOptionMateria #Resultado');
    if (nota.val() < 6)
        resultado.val("Reprobada");
    else
        resultado.val("Aprobada");
}

function eraseInputsFromFormModel() {
    $('#formOptionMateria input, #formOptionMateria select').val('');
    $('#formOptionMateria #Retirada').prop('checked', true);
    $('#formOptionMateria #Retirada').click();
}

function disableInputsFromFormModel(disable) {
    $('#formOptionMateria input, #formOptionMateria select').prop('disabled', disable);
    $('#formOptionMateria #Retirada').prop('checked', true).prop('disabled', disable);
    $('#formOptionMateria #Retirada').click().prop('disabled', disable);
}

$('#formOptionMateria #Codigo').change(function () {
    let asignaturaNombre = $('#formOptionMateria #Codigo option:selected').text();
    $('#formOptionMateria #Asignatura').val(asignaturaNombre);
});

$('#formOptionMateria #Nota').on('keyup', function () {
    actualizarResultadoModal($(this));
});

$('#formOptionMateria #Nota').on('change', function () {
    actualizarResultadoModal($(this));
});

$('#formOptionMateria #Retirada').click(function () {
    if ($(this).prop("checked") == true) {
        $('#formOptionMateria #Resultado').val('Retirada');
        $('#formOptionMateria #Nota').prop('disabled', true);
    }
    else if ($(this).prop("checked") == false) {
        actualizarResultadoModal($('#formOptionMateria #Nota'));
        $('#formOptionMateria #Nota').prop('disabled', false);
    }
});

function shakeResult(object, result, delay) {
    setTimeout(() => {
        object.html(result);
        $($(object)).effect("bounce", {
            times: 5,
            distance: 10
        }, 500, function () {
        });
    }, delay);
}

$('#reestablecerNavItem').click(function () {
    $('#reestablecerModal').modal('show');
});

$('#submitReestablecerBtn').click(function () {
    $.ajax({
        url: '/Expediente/ReestablecerExpediente',
        type: 'post',
        dataType: 'json',
        success: function (data) {
            var controller = window.location.pathname.split("/")[1];
            if (controller.toLowerCase() == 'expediente')
                expedienteTbl.ajax.reload(null, false);
            UpdateResultados();
        }
    });
});

$('#cumDeseadoNavItem').click(function () {
    $('#cumDeseadoModal').modal('show');
});

$('#submitCumDeseadoBtn').click(function () {
    cumDeseado = $('#cumDeseadoInput').val();
    if (cumDeseado != null && !cumDeseado == '')
    {
        $.ajax({
            url: '/Expediente/GetCumDeseado',
            type: 'post',
            dataType: 'json',
            data: { cumDeseado },
            success: function (resultado) {
                MostrarAlerta(resultado.clase, resultado.texto);
            }
        });
    }
});

$('#uvGanadas').click(function () {
    reloadExpediente();
});

function reloadExpediente() {
    $('#expedienteTbl_paginate .current').click();
}