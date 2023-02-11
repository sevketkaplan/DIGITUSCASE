
let reservations_table = null;
let chauffeurs_table = null;

$(function () {
    // =============== Filtrelemede kullanılan select2 yapısı ================    
    $('.js-example-basic-multiple').select2();
    let locationOptions = locations.map(item => {
        let new_option = new Option(item.name, item.id, false, false);
        return new_option;
    });
    $('#locations').append(locationOptions).trigger('change');

    let chauffeurOptions = chauffeurs.map(item => {
        let name_surname = item?.name + " " + item?.surname;
        let new_option = new Option(name_surname, item.id, false, false);
        return new_option;
    });
    $('#chauffeurs').append(chauffeurOptions).trigger('change');

    $('#locations').change(function (e) {
        $('#chauffeurs').html(null);
        let chauffeurOptions = chauffeurs.map(item => {
            let selected = $('#locations').val();

            let name_surname = item?.name + " " + item?.surname;
            let new_option = new Option(name_surname, item.id, false, false);

            if (selected && selected.length > 0) {
                let state = selected.find(i => i == item.locationId);
                if (!state) { new_option.disabled = true }
            }
            return new_option;
        });
        $('#chauffeurs').append(chauffeurOptions).trigger('change');
    });

    $('#trackSelectedChauffeur').click(function (e) {
        let selectedChauffeurs = $('#chauffeurs').val();
        let selectedLocations = $('#locations').val();
        if (selectedChauffeurs && selectedChauffeurs.length > 0) {
            chauffeurMmarkers.forEach(marker => {
                let state = selectedChauffeurs.find(i => i == marker.id);
                if (!state) {
                    marker.setVisible(false);
                    let line = lines.find(i => i.id == marker.id);
                    if (line) {
                        line.setVisible(false);
                    }
                    let chauffeur_info = chauffeurInfoWindows.find(i => i.id == marker.id);
                    if (chauffeur_info) {
                        chauffeur_info.close();
                    }
                }
                else {
                    marker.setVisible(true);
                    let line = lines.find(i => i.id == marker.id);
                    if (line) {
                        line.setVisible(true);
                    }
                }
            });
        }
        else if (selectedLocations && selectedLocations.length > 0 && selectedChauffeurs.length == 0) {
            chauffeurMmarkers.forEach(marker => {
                let state = selectedLocations.find(i => i == marker.locationId);
                if (!state) {
                    marker.setVisible(false);
                    let line = lines.find(i => i.id == marker.id);
                    if (line) {
                        line.setVisible(false);
                    }
                    let chauffeur_info = chauffeurInfoWindows.find(i => i.id == marker.id);
                    if (chauffeur_info) {
                        chauffeur_info.close();
                    }
                }
                else {
                    marker.setVisible(true);
                    let line = lines.find(i => i.id == marker.id);
                    if (line) {
                        line.setVisible(true);
                    }
                }
            });
        }
        else {
            chauffeurMmarkers.forEach(marker => marker.setVisible(true));
            lines.forEach(line => line.setVisible(true));
        }
    });


    // =============== Rezervasyon ve Şoför durum tabloları ================    

    try {
        reservations_table = $('#reservations_table').DataTable(
            {
                "responsive": true,
                "paging": true,
                "filter": true,
                /*"scrollX": true,*/
                "data": reservations.filter(i => i.state == Odeme_yapildi_bekleniyor),

                "columns": [
                    { "name": "#Id", "data": "id", "autoWidth": true, "orderable": true },
                    {
                        "name": "Müşteri", "data": "customer", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let user = `${row?.customer?.name} ${row?.customer?.surname}`;
                            return user;
                        },
                    },
                    { "name": "Durum", "data": "state", "autoWidth": true, "visible": false, "orderable": false },
                    {
                        "name": "Ödeme Numarası", "data": "reservationNumber", "autoWidth": true, "visible": true, "orderable": false,
                    },
                    {
                        "name": "Ödeme ID", "data": "paymentId", "autoWidth": true, "visible": false, "orderable": false,
                    },
                    {
                        "name": "Mesafe(Km)", "data": "distance", "autoWidth": true, "orderable": true,
                        "render": (data, type, row, meta) => {
                            let str = row.distance;
                            return str;
                        }
                    },
                    {
                        "name": "Fiyat(TL)", "data": "totalPrice", "autoWidth": true, "orderable": true,
                        "render": (data, type, row, meta) => {
                            let str = row.totalPrice + " TL";
                            return str;
                        }
                    },
                    {
                        "name": "Kayıt Tarihi", "data": "createAt", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let str = moment(row?.createAt).format('DD-MM-YYYY HH:mm:ss');
                            return str;
                        }
                    },
                    {
                        "data": "delay",
                        "name": "Müşteri İstediği Kalkış Tarih-saati",
                        "render": (data, type, row, meta) => {
                            let date = "Mobil Tarafında İstenen Gecikme Tespit Edilemedi."
                            if (row.delay && row.delay > 10) {
                                let new_date = new Date(new Date(row.createAt).getTime() + row.delay * 60000);
                                date = new_date ? moment(new_date).format("DD-MM-YYYY HH:mm") : "";
                            }
                            return date;
                        },
                        orderable: true,
                    },
                    {
                        "data": "travelDate",
                        "name": "Atanan Kalkış Tarih-saat",
                        "render": (data, type, row, meta) => {
                            let date = row.travelDate ? moment(row.travelDate).format("DD-MM-YYYY HH:mm") : "";
                            return date;
                        },
                        orderable: true,
                    },
                    {
                        "name": "Son Güncelleme", "data": "updateAt", "autoWidth": true, "orderable": true,
                        "render": (data, type, row, meta) => {
                            let str = moment(row?.updateAt).format('DD-MM HH:mm:ss');
                            return str;
                        }
                    },
                    {
                        "name": "Şoför", "data": "chauffeur", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {

                            let user = "";
                            if (row?.chauffeur?.name) {
                                user = `${row.chauffeur.name} ${row.chauffeur.surname}`;
                            }

                            return user;
                        }
                    },
                    {
                        "name": "(Şoför)Lokasyon", "data": null, "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let location = "";
                            if ([Sofor_atandi, Varis_noktasina_gidiliyor].includes(row?.state)) {
                                location = row?.chauffeur?.location?.name;
                            }
                            return location ?? "";
                        }
                    },
                    {
                        "name": "(Müşteri)Lokasyon", "data": "customer", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let user = row?.customer?.location?.name;
                            return user ?? "";
                        }
                    },
                    {
                        "name": "Kalkış Adresi", "data": "originAddress", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let o = `<span title="${row.originAddress}">${row.originAddress && row.originAddress.length > 25 ? row.originAddress + '...' : row.originAddress}</span> `;
                            return o;
                        }
                    },
                    {
                        "name": "Varış Adresi", "data": "destinationAddress", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let d = `<span title="${row.destinationAddress}">${row.destinationAddress && row.destinationAddress.length > 25 ? row.destinationAddress + '...' : row.destinationAddress}</span> `;
                            return d;
                        }
                    },
                    {
                        "name": "İşlemler", "data": null, "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let track_icon = "/js/map/img/marker.png";
                            let track_state = false;
                            let origin_marker = reservationOriginMarkers.find(i => i.id == row.id);
                            if (origin_marker && origin_marker.getVisible()) {
                                track_icon = "/js/map/img/marker_cancel.png";
                                track_state = true;
                            }
                            let str = ``;
                            if (row.state == Odeme_yapildi_bekleniyor || row.state == Sofor_atandi) {
                                str += `<a href="#" onclick="openAssignChauffeurToReservation(${row.id})" title="Şoför Atama" class="btn btn-sm btn-outline-primary"><i class="icon-pencil"></i></a>`;
                            }
                            if (row.state == Odeme_yapildi_bekleniyor || row.state == Sofor_atandi || row.state == Varis_noktasina_gidiliyor || row.state == Iptal_isteği_atildi || row.state == Tamamlandi || row.state == Tamamlanmadi) {
                                str += `<a href="#" onclick="canselPayment(${row.id})" title="İptal" class="btn btn-sm btn-outline-danger"><i class="fas fa-times"></i></a>`;
                            }
                            if (row.state == Iptal_isteği_atildi) {
                                str += `<a href="#" onclick="reservationCancelIsNotValid(${row.id})" title="Reddet" class="btn btn-sm btn-outline-primary"><i class="icon-reset"></i></a>`;
                            }
                            if (row.state == Sofor_atandi || row.state == Varis_noktasina_gidiliyor) {
                                str += `<a href="#" onclick="setReservationIsNotCompleted(${row.id})" title="Tamamlanmadı Moda Çek" class="btn btn-sm btn-outline-warning"><i class="icon-minus-circle2"></i></a>`;
                            }

                            str += `<a href="#" onclick="toggleRouteState(${row.id}, ${track_state})" title="${!track_state ? 'Göster' : 'Gizle'}" class="btn btn-sm btn-outline-secondary"><img src="${track_icon}" style="width:15px;heigth:13px;" ></a>`;
                            return str;
                        }
                    }
                ],
                //"drawCallback": function (setting) {
                //},
                "createdRow": function (row, data, index) {
                    $(row).attr('id', `id${data.id}`);
                    if (data.state == Odeme_yapildi_bekleniyor) {
                        $(row).addClass('selected');
                        let now = new Date();
                        let new_time = (new Date(data.createAt)).getTime() + (data.delay * 60 * 1000);
                        let new_date = new Date(new_time);
                        if (new_date.getTime() - now.getTime() < 3600 * 1000) {
                            $(row).removeClass('selected');
                            $(row).addClass('warning');
                        }
                        if (new_date.getTime() - now.getTime() < 0) {
                            $(row).removeClass('warning');
                            let f = true;
                            setInterval(() => {
                                f = !f;
                                if (f) {
                                    $(row).addClass('danger');
                                }
                                else {
                                    $(row).removeClass('danger');
                                }

                            }, 1000)
                        }

                    }


                },
                "language": {
                    url: "/ext_lib/data-table/tr.json"
                },
                "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Hepsi"]],
                "order": [[0, 'desc']],
                "dom": 'lBftip',
                "buttons": [
                    //{
                    //    extend: 'print',
                    //    exportOptions: {
                    //        columns: ':visible'
                    //    },

                    //},
                    {
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible'
                        }
                    },
                    {
                        extend: 'pdfHtml5',
                        exportOptions: {
                            columns: ':visible'
                        },
                        orientation: 'landscape',
                        pageSize: 'TABLOID',
                        customize: function (doc) {
                            var tblBody = doc.content[1].table.body;
                            // ***
                            //This section creates a grid border layout
                            // ***
                            doc.content[1].layout = {
                                hLineWidth: function (i, node) {
                                    return (i === 0 || i === node.table.body.length) ? 2 : 1;
                                },
                                vLineWidth: function (i, node) {
                                    return (i === 0 || i === node.table.widths.length) ? 2 : 1;
                                },
                                hLineColor: function (i, node) {
                                    return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
                                },
                                vLineColor: function (i, node) {
                                    return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
                                }
                            };
                        }
                    },
                    {
                        extend: 'colvis',
                        text: 'Göster/Gizle'
                    },

                ]
            }
        );
    }
    catch (err) {
        alert(err);
    }
    
    try {

        chauffeurs_table = $('#chauffeurs_table').DataTable(
            {
                "responsive": true,
                "paging": true,
                "filter": true,
                /* "scrollX": true,*/
                "data": chauffeurs.filter(i => i.chauffeurState == Sofor_bos),
                "columns": [
                    {
                        "name": "#Id", "data": "id", "autoWidth": true, "orderable": true
                    },
                    {
                        "name": "Şoför", "data": "name", "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let user = `${row?.name} ${row?.surname}`;
                            return user;
                        }
                    },
                    {
                        "name": "Durum", "data": "chauffeurState", "visible": false, "orderable": false
                    },
                    {
                        "name": "Son Güncelleme", "data": "updateAt", "autoWidth": true, "orderable": true,
                        "render": (data, type, row, meta) => {
                            let str = moment(row?.updateAt).format('DD-MM HH:mm:ss');
                            return str;
                        }
                    },
                    {
                        "name": "Lokasyon", "data": "location", "autoWidth": true, "orderable": true,
                        "render": (data, type, row, meta) => {
                            let str = row?.location?.name;
                            return str;
                        }
                    },
                    {
                        "name": "Tel", "data": "phone", "autoWidth": true, "orderable": false
                    },

                    {
                        "name": "İşlemler", "data": null, "autoWidth": true, "orderable": false,
                        "render": (data, type, row, meta) => {
                            let track_icon = "/js/map/img/marker.png";
                            let track_state = false;
                            let marker = chauffeurMmarkers.find(i => i.id == row.id);
                            if (marker && marker.getVisible()) {
                                track_icon = "/js/map/img/marker_cancel.png";
                                track_state = true;
                            }
                            let str = ``;
                            str += `<a href="#" onclick="toggleChauffeurMarkerState(${row.id}, ${track_state})" title="${track_state ? 'Takibi Bırak' : 'Takip Et'}"><img src="${track_icon}" style="width:24px;heigth:24px;" ></a>`;
                            return str;
                        }
                    }
                ],
                //"drawCallback": function (setting) {
                //},
                "createdRow": function (row, data, index) {
                    $(row).attr('title', `Bulunduğu Lokasyon ${data.location?.name ? data.location?.name : 'Tespit Edilemedi'}`);
                    $(row).attr('id', `id${data.id}`);

                    if (data.chauffeurState == Sofor_bos) {
                        $(row).addClass('selected');
                    }
                },
                "language": {
                    url: "/ext_lib/data-table/tr.json"
                },
                "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Hepsi"]],
                "order": [[0, 'asc']],
                "dom": 'lBftip',
                "buttons": [
                    //{
                    //    extend: 'print',
                    //    exportOptions: {
                    //        columns: ':visible'
                    //    },

                    //},
                    {
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible'
                        }
                    },
                    {
                        extend: 'pdfHtml5',
                        exportOptions: {
                            columns: ':visible'
                        },
                        orientation: 'landscape',
                        pageSize: 'TABLOID',
                        customize: function (doc) {
                            var tblBody = doc.content[1].table.body;
                            // ***
                            //This section creates a grid border layout
                            // ***
                            doc.content[1].layout = {
                                hLineWidth: function (i, node) {
                                    return (i === 0 || i === node.table.body.length) ? 2 : 1;
                                },
                                vLineWidth: function (i, node) {
                                    return (i === 0 || i === node.table.widths.length) ? 2 : 1;
                                },
                                hLineColor: function (i, node) {
                                    return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
                                },
                                vLineColor: function (i, node) {
                                    return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
                                }
                            };
                        }
                    },
                    {
                        extend: 'colvis',
                        text: 'Göster/Gizle'
                    },

                ]
            }
        );

    }
    catch (err) {
        alert(err);
    }    
    setTimeout(() => {
        let table_wrapper = {
            display: "flex",
            flexWrap: "wrap",
            justifyContent: "space-between"
        };
        let table_paginate = {
            margin: "auto",
            marginTop: "4px",
            marginRight: "0px"
        };


        $('.dt-buttons button').css("marginRight", "5px");

        $('#reservations_table_wrapper').css(table_wrapper);
        $('#reservations_table_paginate').css(table_paginate);
        $('#reservations_table_length').css("marginLeft", "5px");
        $('#reservations_table_filter').css("marginRight", "5px");
        $('#reservations_table_filter').css("marginTop", "0px");
        $('#reservations_table_info').css("marginLeft", "5px");


        $('#chauffeurs_table_wrapper').css(table_wrapper);
        $('#chauffeurs_table_paginate').css(table_paginate);
        $('#chauffeurs_table_length').css("marginLeft", "5px");
        $('#chauffeurs_table_filter').css("marginRight", "5px");
        $('#chauffeurs_table_filter').css("marginTop", "0px");
        $('#chauffeurs_table_info').css("marginLeft", "5px");
    }, 200);
   
    // =================== Şoför Atama Ekranı  ===================
    $('#assign_chauffeur_to_reservation_form button[type="button"]').click(() => {

        $('#empty_chauffeurs').html(null);
    });

    $('#submit_assign_chauffeur_to_reservation_form').submit((e) => {
        e.preventDefault();
        let form = $('#submit_assign_chauffeur_to_reservation_form');
        let data = new FormData(form[0]);
        let selected_chauffeur_id = $('#empty_chauffeurs').val();
        if (data.get("Id") && selected_chauffeur_id > 0) {
            Swal.fire({
                title: 'Emin misiniz?',
                text: "İşlemi tamamlamak için lütfen evete tıklayınız.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Evet',
                cancelButtonText: 'Hayır'
            }).then((result) => {
                if (result.isConfirmed) {
                    // ============ <Start> Loading  ===========
                    Swal.fire({
                        title: 'Lütfen Bekleyiniz...',
                        allowOutsideClick: false,
                        showConfirmButton: false,
                        didOpen: () => {
                            Swal.showLoading()
                        }
                    });
                    // ============ </End> Loading  ==========
                    let id = data.get("Id");
                    let travelDate = data.get("TravelDate");

                    let reservation = reservations.find(i => i.id == id);
                    if (reservation) {
                        let empty_chauffeur = chauffeurs.find(i => i.id == selected_chauffeur_id);
                        if (empty_chauffeur) {
                            let data = { reservationId: id, reservationState: Sofor_atandi, chauffeurId: empty_chauffeur.id, chauffeurState: Yolad_musteriye_gidiliyor, travelDate };
                            axios({
                                url: '/Tracking/UpdateReservtionAndSelectedChauffeurState',
                                method: 'Post',
                                data: data,
                                headers: { 'Content-Type': 'application/json' }
                            })
                                .then(res => {
                                    if (res && res.data && res.data.isSuccess) {
                                        Swal.fire("DİKKAT", 'İşlem Başarılı.', 'success');
                                    }
                                    else {
                                        Swal.fire("UYARI", res.data?.message, 'warning');
                                    }
                                })
                                .catch(err => {
                                    console.log("err ::::: ", err);
                                    Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
                                });

                        }
                    }
                    $('#assign_chauffeur_to_reservation_form').modal('hide');

                }
            })
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz, şofor bulunmadı", '', 'warning');
        }
    });
    
    const interval_id = setInterval(() => {      //5 dkbir rezervayon tablosunu refreshle
        let res = reservations.filter(i => i.state == Odeme_yapildi_bekleniyor);
        if (!res || res.length == 0) {
            clearInterval(interval_id);
        }
        let tmp = res.filter(i => {
            let now = new Date();
            let new_time = (new Date(i.createAt)).getTime() + (i.delay * 60 * 1000);
            let new_date = new Date(new_time);
            if (new_date.getTime() - now.getTime() < 3600 * 1000) {
                return i;
            }
        });

        if (tmp && tmp.length > 0) {
            reloadReservationData();
        }
    }, 300000);
   

});

function toggleRouteState(id, state) {
    if (!state) { // if state == null | undefind
        state = false;
    }
    if (map && id) {
        toggleRouteLineAndMarkers(id, !state);
        setTimeout(() => {
            reloadReservationData();
        }, 500);
    }

}

function toggleChauffeurMarkerState(id, state) {
    if (map) {

        if (state) {// marker haritadan gizlenecek
            hideTrackingMarker(id);
        }
        else {//marker  haritada gösterilecek            
            let marker = chauffeurMmarkers.find(i => i.id == id);
            if (marker && marker.getPosition()) {
                showTrackingMarker(id);
                map.setCenter(marker.getPosition());
            }
        }
        reloadChauffeurData();

    }

}

function openAssignChauffeurToReservation(id) {
    if (id && id > 0) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.directionsRenderer) {
            $('#empty_chauffeurs').html(null);
            $('.js-example-basic-single').select2();
            let emptyChauffeurOptions = chauffeurs.map(item => {
                let new_option = new Option(`${item?.name} ${item?.surname} - Durum: ${getChauffeurState(item.chauffeurState)} - Lokasyon: ${item?.location?.name}`, item.id, false, false);
                new_option.disabled = true;
                if (item.chauffeurState == Sofor_bos) {
                    new_option.disabled = false;
                }
                return new_option;
            });            
            $('#empty_chauffeurs').append(emptyChauffeurOptions).trigger('change');
            let delay = new Date(new Date(reservation.createAt)?.getTime() + (reservation?.delay * 60 * 1000));
            $('#submit_assign_chauffeur_to_reservation_form input[name="Delay"]').val(moment(delay).format('DD-MM-YYYY HH:mm') ?? 'belirtilmemiş müşteriyle iletişime geç');
            $('#submit_assign_chauffeur_to_reservation_form input[name="Id"]').val(id);
            $('#assign_chauffeur_to_reservation_form').modal('show');
        }
    }
}

function reloadReservationData() { // Redraw Reservations tables by state    
    let state = $('#reservation_state').val();
    if (state) {
        reservations_table.clear();
        let tmp_arr = reservations.filter(i => i.state == state);
        reservations_table.rows.add(tmp_arr).draw();
    }

}

function reloadChauffeurData() { // Redraw Chauffeurs tables by state
    let state = $('#chauffeur_state').val();
    if (state) {
        chauffeurs_table.clear();
        let tmp_arr = chauffeurs.filter(i => i.chauffeurState == state);
        chauffeurs_table.rows.add(tmp_arr).draw();
    }
}


function canselPayment(id) {
    let url = "/Reservation/CancelReservation/" + id;
    Swal.fire({
        title: 'Emin misiniz?',
        text: "Rezervasyon iptal edilsin mi?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır'
    }).then((result) => {
        if (result.isConfirmed) {
            $('#loader-gif').removeClass("non-active");
            axios({
                url: url,
                data: {},
                method: 'Post'
            }).then(res => {
                if (res.data.isSuccess === true) {
                    $('#loader-gif').addClass("non-active");
                    reservations_table.draw();
                    Swal.fire(res.data.message, '', 'success')
                }
                else {
                    $('#loader-gif').addClass("non-active");
                    Swal.fire({
                        title: 'DİKKAT',
                        text: res.data.message,
                        icon: 'warning',
                        showCancelButton: false,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Devam',
                        cancelButtonText: 'Hayır'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $('#loader-gif').addClass("non-active");
                            Swal.fire({
                                title: 'Emin misiniz?',
                                text: "Rezervasyon iptal işlemini şoföriste sisteminde yapılmasını onaylıyor musunuz?",
                                icon: 'question',
                                showCancelButton: true,
                                confirmButtonColor: '#3085d6',
                                cancelButtonColor: '#d33',
                                confirmButtonText: 'Evet',
                                cancelButtonText: 'Hayır'
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    let force_cancel_url = "/Reservation/ForcelyCancelReservation/" + id;
                                    $('#loader-gif').removeClass("non-active");
                                    axios({
                                        url: force_cancel_url,
                                        data: {},
                                        method: 'Post'
                                    }).then(res => {
                                        if (res.data.isSuccess === true) {
                                            $('#loader-gif').addClass("non-active");
                                            reloadReservationData();
                                            Swal.fire(res.data.message, '', 'success')
                                        }
                                        else {
                                            $('#loader-gif').addClass("non-active");
                                            Swal.fire(res.data.message, '', 'warning');

                                        }
                                    }).catch(err => {
                                        $('#loader-gif').addClass("non-active");

                                        Swal.fire('Rezervasyon iptali sırasında hata! Geliştirici ekibe danışınız.', '', 'error');// sen mesajları düzeltirsin tamam mı tamam
                                        console.log('ERR ::::::::  ', err);
                                    });

                                }
                            })

                        }
                    })




                }
            }).catch(err => {
                $('#loader-gif').addClass("non-active");

                Swal.fire('Rezervasyon iptali sırasında hata! Geliştirici ekibe danışınız.', '', 'error');// sen mesajları düzeltirsin tamam mı tamam
                console.log('ERR ::::::::  ', err);
            });

        }
    })
}


function reservationCancelIsNotValid(id) {
    let url = "/Tracking/ReservationCancelIsNotValid/" + id;
    Swal.fire({
        title: 'Emin misiniz?',
        text: "Rezervasyon İptal İsteği Reddedilecek.",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır'
    }).then((result) => {
        if (result.isConfirmed) {
            $('#loader-gif').removeClass("non-active");
            axios({
                url: url,
                data: {},
                method: 'Post'
            }).then(res => {
                if (res.data.isSuccess === true) {
                    $('#loader-gif').addClass("non-active");
                    reservations_table.draw();
                    Swal.fire(res.data.message, '', 'success')
                }
                else {
                    $('#loader-gif').addClass("non-active");

                    Swal.fire(res.data.message, '', 'warning');
                }
            }).catch(err => {
                $('#loader-gif').addClass("non-active");

                Swal.fire('Rezervasyon silme işlemi sırasında hata! Geliştirici ekibe danışınız.', '', 'error');// sen mesajları düzeltirsin tamam mı tamam
                console.log('ERR ::::::::  ', err);
            });

        }
    })
}
function setReservationIsNotCompleted(id) {
    let url = "/Tracking/SetReservationIsNotCompleted/" + id;
    Swal.fire({
        title: 'Emin misiniz?',
        text: "Rezervasyon Tamamlanmadı Moda Geçecek.",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır'
    }).then((result) => {
        if (result.isConfirmed) {
            $('#loader-gif').removeClass("non-active");
            axios({
                url: url,
                data: {},
                method: 'Post'
            }).then(res => {
                if (res.data.isSuccess === true) {
                    $('#loader-gif').addClass("non-active");
                    reservations_table.draw();
                    Swal.fire(res.data.message, '', 'success')
                }
                else {
                    $('#loader-gif').addClass("non-active");

                    Swal.fire(res.data.message, '', 'warning');
                }
            }).catch(err => {
                $('#loader-gif').addClass("non-active");

                Swal.fire('Rezervasyon silme işlemi sırasında hata! Geliştirici ekibe danışınız.', '', 'error');// sen mesajları düzeltirsin tamam mı tamam
                console.log('ERR ::::::::  ', err);
            });

        }
    })
}




