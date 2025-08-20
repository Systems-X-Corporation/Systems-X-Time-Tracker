$(document).ready(function () {


    var privilegios = new Object();

    var modulo_activo = '';

    cargarPrivilegios();

function cargarPrivilegios() {

    $.ajax({
        type: 'GET',
        url: '../../Menu/getAccesoPagina',
        dataType: 'json',
        data:
        {
            "user": '304430140'
        },
        success: function (data) {

            privilegios = data.paginas;

            if (Object.keys(data).length > 0) {
                cargarAreas(privilegios);
            }
        },
        timeout: 30 * 60 * 1000
    }).fail(function (xhr, status) {

        if (status == "timeout") {
            Console.log('Tiempo de respuesta agotado');
        }
        if (status == "error") {
            Console.log('La cantidad de datos excede el limite por conexión');
        }
    });

}

var cargaPaginasDeUrl = false;

function cargarAreas(obj) {

    var areas_cargar = {
        items: []
    };

    /*Obtengo las distintas áreas*/
    for (i in _.groupBy(_.without(obj, _.findWhere(obj, { id_area: 0 })), 'id_area')) {
        var area = _.first(_.filter(obj, function (d) { return d.id_area == i }));

        areas_cargar.items.push(
            {
                area_orden: area.area_orden,
                area_titulo: area.area_titulo,
                id_area: area.id_area,
                area_url_icono: area.area_url_icono
            }
        );

    }
    /*Ordeno las áreas*/
    var areas_ordenadas = _.sortBy(areas_cargar.items, 'area_orden');

    /*Los agrego al menú*/
    for (a in areas_ordenadas) {
        /*Agrego áreas al menú hamburguesa*/
        $('#sidebar-menu').append('<li class="treeview"> < a href = "#" > <i class="' + areas_ordenadas[a].area_url_icono + '" ></i> <span>' + areas_ordenadas[a].area_titulo + ' </span> < span class= "pull-right-container" > <i class="fa fa-angle-left pull-right"></i> </span > </a > <ul class="treeview-area' + areas_ordenadas[a].id_area + '" > </ul> </li > ');

        id_area = areas_ordenadas[a].id_area;

        var modulos_cargar = {
            items: []
        };

        /*Agrupo para obtener los distintos módulos*/
        for (i in _.groupBy(privilegios, 'id_modulo')) {

            /*Como hay muchas páginas por módulo solo obtengo el primero para extraer la información del módulo.*/
            var modulo = _.first(_.filter(privilegios, function (d) { return d.id_modulo == i }));

            /*Verifico el área del módulo para colocarlo o no en el mosaico.*/
            if (modulo.id_area == id_area) {

                modulos_cargar.items.push(
                    {
                        modulo_orden: modulo.modulo_orden,
                        id_modulo: modulo.id_modulo,
                        modulo_abreviatura: modulo.modulo_abreviatura,
                        modulo_url_icono: modulo.modulo_url_icono,
                        modulo_titulo: modulo.modulo_titulo
                    }
                );

            }

        }
        /*Ordeno los módulos*/
        var modulos_ordenados = _.sortBy(modulos_cargar.items, 'modulo_orden');
        /*Los agrego al menú*/
        for (m in modulos_ordenados) {

            /*Acá ingreso los módulos*/
           // $('#ulbtnarea-' + id_area + '').append('<li><img src="' + modulos_ordenados[m].modulo_url_icono + '" class="img_modulo_hamburguesa" alt= "img"> <button id="btnmodulo-' + modulos_ordenados[m].id_modulo + '" class="titulo-modulo-menu_hamburguesa">' + modulos_ordenados[m].modulo_titulo + '</button><ul style = "display:none;"  id="ulbtnmodulo-' + modulos_ordenados[m].id_modulo + '"></ul> <div class="separador_hamburguesa"></div></li ></div> ');

            $('#treeview-area' + id_area + '').append('<li class="treeview"> < a href = "#" > <i class="' + modulos_ordenados[m].modulo_url_icono + '" ></i> <span>' + modulos_ordenados[m].id_modulo + ' </span> < span class= "pull-right-container" > <i class="fa fa-angle-left pull-right"></i> </span > </a > <ul class="treeview-modulo' + modulos_ordenados[m].id_modulo + '" > </ul> </li > ');


            //console.log(id_area);
            id_modulo = modulos_ordenados[m].id_modulo;

            for (i in privilegios) {
                if (privilegios[i].id_modulo == id_modulo) {

                    $('#treeview-modulo-' + id_modulo).append('<li class="@Url.IsLinkActive("' + privilegios[i].controller + '", "' + privilegios[i].action + '")" > <a href="@Url.Action("' + privilegios[i].controller + '", "' + privilegios[i].action + '")" > <i class="' + privilegios[i].url + '"></i>' + privilegios[i].titulo + '</a ></li >');

                    
                }
            }
        }
    }

    /*Creo el evento click para cargar los modulos cuando el usuario cambie de área*/
    //$('.titulo-area-menu_hamburguesa, .img_area_hamburguesa').click(function () {

    //    var id = this.id;
    //    //console.log(id);
    //    if ($('#ul' + id).children('li').length === 0) {
    //        for (a in areas_ordenadas) {
    //            if (areas_ordenadas[a].id_area == id.split("-")[1]) {
    //                $('#ul' + id).css("display", "");
    //            }
    //            else {
    //                $('#ulbtnarea-' + areas_ordenadas[a].id_area).css("display", "none");
    //            }
    //        }

    //    }
    //    else {
    //        if ($('#ul' + id).css('display') === "none") {
    //            $('#ul' + id).css("display", "");
    //            for (a in areas_ordenadas) {

    //                if (areas_ordenadas[a].id_area == id.split("-")[1]) {
    //                    $('#ul' + id).css("display", "");
    //                }
    //                else {
    //                    $('#ulbtnarea-' + areas_ordenadas[a].id_area).css("display", "none");
    //                }
    //            }
    //        }
    //        else {
    //            $('#ul' + id).css("display", "none");
    //        }
    //    }

    //});

    //$('.titulo-modulo-menu_hamburguesa, .img_modulo_hamburguesa').click(function () {

    //    var id = this.id;

    //    if ($('#ul' + id).css('display') == 'none') {
    //        $('#ul' + id).css('display', '');

    //        for (i in privilegios) {

    //            if (privilegios[i].id_modulo == id.split("-")[1]) {
    //                $('#ul' + id).css('display', '');
    //            }
    //            else {
    //                $('#ulbtnmodulo-' + privilegios[i].id_modulo).css('display', 'none');
    //            }
    //        }
    //    }
    //    else {
    //        $('#ul' + id).css('display', 'none');
    //    }

    //});

}
});