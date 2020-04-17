var expedienteTbl;
$(document).ready(function () {
    expedienteTbl = $('#expedienteTbl').DataTable({
        "language": {
            "lengthMenu": "Mostrar _MENU_ materias por pagina",
            "zeroRecords": "Nada encontrado - lo siento.",
            "info": "Mostrando pagina _PAGE_ de _PAGES_",
            "sSearch": "Buscar:",
            "infoEmpty": "No hay materias disponibles",
            "infoFiltered": "(filtrados de las _MAX_ materias totales)",
            "sLoadingRecords": "Cargando...",
            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            }
        },
        "ajax": {
            "url": "/Expediente/GetJsonMateriasExpedientes",
            "type": "POST",
            "datatype": "json",
            "dataSrc": function (json) {
                return json.data;
            },
        },
        "columns": [
            { "data": "anio", "name": "anio" },
            { "data": "ciclo", "name": "ciclo" },
            { "data": "codigo", "name": "codigo" },
            { "data": "asignatura", "name": "asignatura" },
            { "data": "matricula", "name": "matricula" },
            { "data": "nota", "name": "nota" },
            { "data": "resultado", "name": "resultado" },
            {
                defaultContent: '<button id="editMateriaBtn" class="btn btn-warning">Editar</button>'
                              + '<button id="deleteMateriaBtn" class="btn btn-danger ml-2">Eliminar</button>'
            }
        ],
        "serverSide": "true",
        "order":[0,"asc"],

    });
});