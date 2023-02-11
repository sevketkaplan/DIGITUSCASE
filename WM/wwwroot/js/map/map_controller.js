"use strict";
let map;  // Ana harita

let chauffeurs = [];//Şoför Listesi 
let locations = [];  // Lokasyon Listesi

let lines = [];// Şoförlerin takip Çizgi Listesi
let chauffeurMmarkers = [];// Şoförler Marker Listesi

let reservations = [];// Rezervasyon Listesi
let reservationOriginMarkers = []; // Rezervasyonlar Başlangıç Marker Listesi
let reservationDestinationMarkers = []; // Rezervasyonlar Bitiş Marker Listesi

let originInfoWindows = []; // Rezervasyonların Başlangıç Marker İnfo'su Listesi
let destinationInfoWindows = [];// Rezervasyonların Bitiş Marker İnfo'su  Listesi
let chauffeurInfoWindows = []; // Şoförlerin Marker İnfo'su  Listesi

let defaultChauffeurProperty = {// varsayılan Şoför Marker'i ve Takip Çizgisinin özellikleri
    line: {
        visible: false, // takip çizgisini göster
        color: "#000",
        strokeOpacity: 1.0,
        strokeWeight: 3,
    },
    marker: {
        visible: true,// bulunduğu konumu marker olarak göster
        icon: "/js/map/img/car.png"
    }
};
let defaultOriginMarkerIconOption = {// varsayılan Rezervasyonların Başlangıç Marker'inin ikon özellikleri
    color: "#000",
    opacity: 0.6
};
let defaultDestinationMarkerIconOption = {// varsayılan Rezervasyonların Bitiş Marker'inin ikon özellikleri
    color: "#0ec72d",
    opacity: 0.6
};
let defaultRouteLineOption = {// varsayılan Route'nin çizgi özellikleri
    color: "#54f542",
    strokeOpacity: 1.0,
    strokeWeight: 3,
};


function initMap() { // Sayfa ilk açıldığında harita yapısını oluşturma

    let AntalayaCenter = { lat: 36.9081, lng: 30.6956 }; // Ana harita Merkezi
    let options = { // Harita Özellikleri
        zoom: 5,
        mapTypeId: 'roadmap', // roadmap | hybrid,
        center: AntalayaCenter,
        mapTypeControl: true,
        scaleControl: true,
        panControl: false,
        fullscreenControl: true,
        navigationControl: true,
        streetViewControl: false
    };
    map = new google.maps.Map(document.getElementById("map"), options);// Ana Harita Oluşturma 
    //if (reservations.length > 0) {// Rezervasyonların Oluşturacağı Route'lari Haritada Gösterme 
    //    reservations.forEach(reservation => {
    //        reservation.locationId = reservation.customer.locationId; //Rezervasyonlarda müşteri baz alınarak lokasyon atanıyor
    //        reservation.directionsRenderer = calculateAndDisplayRoute(reservation);
    //        reservation.lineOption = defaultRouteLineOption;
    //        reservation.originMarkerOption = {
    //            title: getOriginMarkerTitle(reservation),
    //            icon: defaultOriginMarkerIconOption,
    //        };
    //        reservation.destinationMarkerOption = {
    //            title: getDestinationMarkerTitle(reservation),
    //            icon: defaultDestinationMarkerIconOption,

    //        };
    //    });
    //}
    if (chauffeurs.length > 0) {// Şoför Marker'lerini ve Takip Çizgilerini Ekleme
        chauffeurs.forEach(chauffeur => {
            let line = new google.maps.Polyline({ // çizgileri elde ettim
                id: chauffeur.id, // bu property benim tarafımdan eklendi normalde id bu yapıda yok
                locationId: chauffeur.locationId,
                map: map,
                path: chauffeur.directionList,
                strokeColor: (chauffeur.property?.line?.color) ? chauffeur.property.line.color : defaultChauffeurProperty.line.color,
                strokeOpacity: (chauffeur.property?.line?.strokeOpacity) ? chauffeur.property.line.strokeOpacity : defaultChauffeurProperty.line.strokeOpacity,
                strokeWeight: (chauffeur.property?.line?.strokeWeight) ? chauffeur.property.line.strokeWeight : defaultChauffeurProperty.line.strokeWeight,
                visible: (chauffeur.property) ? chauffeur.property.line.visible : defaultChauffeurProperty.line.visible
            });

            let marker = new google.maps.Marker({ // markerleri elde ettim
                id: chauffeur.id,// bu property benim tarafımdan eklendi normalde id bu yapıda yok
                locationId: chauffeur.locationId,
                map: map,
                position: (chauffeur.directionList && chauffeur.directionList.length > 0) ? chauffeur.directionList[chauffeur.directionList.length - 1] : null,
                title: `${chauffeur?.name} ${chauffeur?.surname} - Durumu: ${getChauffeurState(chauffeur?.chauffeurState)}`,
                icon: defaultChauffeurProperty.marker.icon,
                animation: google.maps.Animation.DROP,
                visible: (chauffeur.property && chauffeur.property.marker) ? chauffeur.property.marker.visible : defaultChauffeurProperty.marker.visible,
            });

            const infowindow = new google.maps.InfoWindow({ // Şoför markerine tıklandığında şoför infoWİndow
                content: getChauffeurInfoWindowCotent(chauffeur),
                id: chauffeur.id
            });
            chauffeurInfoWindows.push(infowindow);
            marker.addListener("click", () => { // Şoför markerine tıklandığında
                infowindow.open({
                    anchor: marker,
                    map,
                    shouldFocus: false,
                });
            });

            if (!chauffeur.property) {
                if (chauffeur.chauffeurState == Sofor_bos) { // mesai dışı yada boşta olan şoförler harita ilk açıldığında harita üzerinde gösterilmeyecek
                    line.setVisible(false);
                    marker.setVisible(false);
                }
            }


            lines.push(line);
            chauffeurMmarkers.push(marker);
        });
    }
}

function calculateAndDisplayRoute(reservation, visible_state) {// Rezervasyon route'sini oluşturma

    const directionsService = new google.maps.DirectionsService();
    const directionsRenderer = new google.maps.DirectionsRenderer();


    let request = {
        origin: reservation.origin, //LatLng | String | google.maps.Place,
        destination: reservation.destination, //LatLng | String | google.maps.Place,
        travelMode: 'DRIVING',//TravelMode = DRIVING | BICYCLING | TRANSIT | WALKING ,
        //transitOptions: //TransitOptions,
        /*drivingOptions: {
            departureTime: Date,
            trafficModel: "bestguess" // bestguess | pessimistic | optimistic
        },*///DrivingOptions,
        unitSystem: google.maps.UnitSystem.METRIC,//UnitSystem = METRIC | IMPERIA,
        //waypoints[]: DirectionsWaypoint,
        //optimizeWaypoints: true, //Boolean,
        //provideRouteAlternatives: true, //Boolean,
        //avoidFerries: Boolean,
        //avoidHighways: Boolean,
        //avoidTolls: Boolean,
        //region: String
    };

    directionsService
        .route(request, (response, status) => {
            if (status == "OK") {
                let myRoute = response.routes[0].legs[0];
                // ========== Route alakalı tüm bilgileri direction objesi içinde tut    
                addRouteDataToDirectionInfo(reservation.id, myRoute);
                directionsRenderer.setDirections(response);
                let visible = false;
                if (reservation.state == Sofor_atandi || reservation.state == Varis_noktasina_gidiliyor) visible = true;
                if (visible_state) {
                    visible = visible_state;
                }
                directionsRenderer.setOptions({
                    routeIndex: reservation.id,
                    suppressMarkers: true, // default origin ve destination markerleri engelleme        
                    polylineOptions: {
                        visible: visible,
                        strokeColor: defaultRouteLineOption?.color,
                        strokeOpacity: defaultRouteLineOption?.strokeOpacity,
                        strokeWeight: defaultRouteLineOption?.strokeWeight,
                    },
                });
                // ========= Yeni Origin Ve Destination Markerlerini oluşturma
                makeRouteOriginMarker(reservation.id, reservation.origin, visible); //myRoute.start_location
                makeRouteDestinationMarker(reservation.id, reservation.destination, visible);    // myRoute.end_location
                // ========== haritada yansıt
                directionsRenderer.setMap(map);

            }
            else {
                directionsRenderer.setDirections(response);
                directionsRenderer.setMap(null);
            }
        })
        .catch((e) => window.alert("Directions request failed due to " + e));

    return directionsRenderer;
}
function addRouteDataToDirectionInfo(id, data) {// google'den gelen tüm Route biliglerini direction içinde tutma
    let reservation = reservations.find(i => i.id == id);
    if (reservation && reservation.id > 0)
        reservation.data = data
}

function makeRouteOriginMarker(id, position, visible) {// Route'nin başlangıç noktasını oluşturmak
    let reservation = reservations.find(i => i.id == id);
    let default_origin_title = reservation?.originMarkerOption?.title;

    let marker = new google.maps.Marker({
        position: position,
        map: map,
        icon: routeOriginMarker(null),
        title: default_origin_title,
        animation: google.maps.Animation.DROP,
        id: id,
        visible: visible,
    });
    if (reservation && (reservation.state == Sofor_atandi || reservation.state == Varis_noktasina_gidiliyor)) {
        marker.setVisible(true);
    }

    const infowindow = new google.maps.InfoWindow({
        content: getRouteOriginInfoWIndowContent(reservation),
        id: id
    });
    originInfoWindows.push(infowindow);
    marker.addListener("click", () => {
        infowindow.open({
            anchor: marker,
            map,
            shouldFocus: false,
        });
    });

    reservationOriginMarkers.push(marker);

}
function makeRouteDestinationMarker(id, position, visible) {// Route'nin bitiş noktasını oluşturmak
    let reservation = reservations.find(i => i.id == id);
    let default_destination_title = reservation?.destinationMarkerOption?.title;
    let marker = new google.maps.Marker({
        position: position,
        map: map,
        icon: routeDestinationMarker(null),
        title: default_destination_title,
        animation: google.maps.Animation.DROP,
        id: id,
        visible: visible
    });
    if (reservation && (reservation.state == Sofor_atandi || reservation.state == Varis_noktasina_gidiliyor)) {
        marker.setVisible(true);
    }
    const infowindow = new google.maps.InfoWindow({
        content: getRouteDestinationInfoWIndowContent(reservation),
        id: id
    });
    destinationInfoWindows.push(infowindow);
    marker.addListener("click", () => {
        infowindow.open({
            anchor: marker,
            map,
            shouldFocus: false,
        });
    });
    reservationDestinationMarkers.push(marker);
}
function editRouteOriginMarker(marker, option) {// Route'nin başlangıç marker'ini tekrar ayarlamak    
    if (marker && marker.id > 0) {
        if (option && option.icon && option.icon.color && option.icon.color.length > 2 && option.icon.opacity && option.icon.opacity >= 0 && option.icon.opacity <= 1) {
            let icon = routeOriginMarker(option.icon);
            let reservation = reservations.find(i => i.id == marker.id);
            reservation.originMarkerOption.icon = option.icon;
            marker.setIcon(icon);

            //let info_origin = originInfoWindows.find(i => i.id == reservation.id);
            //if (info_origin) {
            //    info_origin.setContent(getRouteOriginInfoWIndowContent(reservation));
            //}
        }
    }
}
function editRouteDestinationMarker(marker, option) {// Route'nin bitiş marker'ini tekrar ayarlamak    
    if (marker && marker.id > 0) {
        if (option && option.icon && option.icon.color && option.icon.color.length > 2 && option.icon.opacity && option.icon.opacity >= 0 && option.icon.opacity <= 1) {
            let icon = routeDestinationMarker(option.icon);
            let reservation = reservations.find(i => i.id == marker.id);
            reservation.destinationMarkerOption.icon = option.icon;
            marker.setIcon(icon);
            //let info_dest = destinationInfoWindows.find(i => i.id == reservation.id);
            //if (info_dest) {
            //    info_dest.setContent(getRouteDestinationInfoWIndowContent(reservation));
            //}
        }
    }
}
function routeOriginMarker(option) {// Route'nin başlangıç marker yapısı   
    let svgMarker = {
        path: "M172.268 501.67C26.97 291.031 0 269.413 0 192 0 85.961 85.961 0 192 0s192 85.961 192 192c0 77.413-26.97 99.031-172.268 309.67-9.535 13.774-29.93 13.773-39.464 0zM192 272c44.183 0 80-35.817 80-80s-35.817-80-80-80-80 35.817-80 80 35.817 80 80 80z",
        fillColor: (option && option.color && option.color.length > 2) ? option.color : defaultOriginMarkerIconOption.color,
        fillOpacity: (option && option.opacity && option.opacity >= 0 && option.opacity <= 1) ? parseFloat(option.opacity) : defaultOriginMarkerIconOption.opacity, // 0.6,
        strokeWeight: 0,
        rotation: 0,
        scale: 0.075,
        anchor: new google.maps.Point(200, 500),
    };
    return svgMarker;
}
function routeDestinationMarker(option) {// Route'nin bitiş marker yapısı
    let svgMarker = {
        path: "M172.268 501.67C26.97 291.031 0 269.413 0 192 0 85.961 85.961 0 192 0s192 85.961 192 192c0 77.413-26.97 99.031-172.268 309.67-9.535 13.774-29.93 13.773-39.464 0zM192 272c44.183 0 80-35.817 80-80s-35.817-80-80-80-80 35.817-80 80 35.817 80 80 80z ",
        fillColor: (option && option.color && option.color.length > 2) ? option.color : defaultDestinationMarkerIconOption.color,
        fillOpacity: (option && option.opacity && option.opacity >= 0 && option.opacity <= 1) ? parseFloat(option.opacity) : defaultDestinationMarkerIconOption.opacity, // 0.6,
        strokeWeight: 0,
        rotation: 0,
        scale: 0.075,
        anchor: new google.maps.Point(200, 500),
    };
    return svgMarker;
}
function changeRouteLine(directionsRenderer, option) { // Route'nin çizgisinin renk va kalınlığını ayarlamak
    directionsRenderer.setOptions({
        polylineOptions: {
            strokeColor: (option && option.color && option.color.length > 2) ? option.color : defaultRouteLineOption.color,
            strokeOpacity: (option && option.strokeOpacity && option.strokeOpacity >= 0 && option.strokeOpacity <= 1) ? parseFloat(option.strokeOpacity) : defaultRouteLineOption.strokeOpacity,
            strokeWeight: (option && option.strokeWeight && option.strokeWeight >= 0 && option.strokeWeight <= 10) ? parseFloat(option.strokeWeight) : defaultRouteLineOption.strokeWeight
        }
    });
    directionsRenderer.setMap(map);

    let reservation = reservations.find(i => i.id == directionsRenderer.routeIndex);
    if (option && option.color && option.color.length > 2) reservation.lineOption.color = option.color;
    if (option && option.strokeOpacity && option.strokeOpacity >= 0 && option.strokeOpacity <= 1) reservation.lineOption.strokeOpacity = parseFloat(option.strokeOpacity);
    if (option && option.strokeWeight && option.strokeWeight >= 0 && option.strokeWeight <= 10) reservation.lineOption.strokeWeight = parseFloat(option.strokeWeight);
}
function hideRouteLineAndMarkers(id) {// Rezervasyonu haritada gizle
    if (id && id > 0) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.id > 0 && reservation.directionsRenderer) {
            reservation.directionsRenderer.setOptions({
                polylineOptions: {
                    visible: false
                }
            });
            reservation.directionsRenderer.setMap(map);
            let origin_marker = reservationOriginMarkers.find(i => i.id == id);
            if (origin_marker) {
                origin_marker.setVisible(false);
            }
            let destination_marker = reservationDestinationMarkers.find(i => i.id == id);
            if (destination_marker) {
                destination_marker.setVisible(false);
            }

            reloadReservationData();
        }
        let origin_info = originInfoWindows.find(i => i.id == id);
        if (origin_info) {
            origin_info.close();
        }
        let destination_info = destinationInfoWindows.find(i => i.id == id);
        if (destination_info) {
            destination_info.close();
        }
    }
}
function toggleRouteLineAndMarkers(id, state) {// rezervasyonu haritada göster/ gizle
    if (id && id > 0) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.directionsRenderer) {
            reservation.directionsRenderer.setOptions({
                polylineOptions: {
                    visible: state,
                    strokeColor: reservation.lineOption?.color,
                    strokeOpacity: reservation.lineOption?.strokeOpacity,
                    strokeWeight: reservation.lineOption?.strokeWeight,

                }
            });
            reservation.directionsRenderer.setMap(map);
        }
        if (!reservation.directionsRenderer) {
            reservation.locationId = reservation.customer.locationId; //Rezervasyonlarda müşteri baz alınarak lokasyon atanıyor
            reservation.directionsRenderer = calculateAndDisplayRoute(reservation, true);
            reservation.lineOption = defaultRouteLineOption;
            reservation.originMarkerOption = {
                title: getOriginMarkerTitle(reservation),
                icon: defaultOriginMarkerIconOption,
            };
            reservation.destinationMarkerOption = {
                title: getDestinationMarkerTitle(reservation),
                icon: defaultDestinationMarkerIconOption,

            };
        }

        let origin_marker = reservationOriginMarkers.find(i => i.id == id);
        if (origin_marker) {
            origin_marker.setVisible(state);
        }
        let destination_marker = reservationDestinationMarkers.find(i => i.id == id);
        if (destination_marker) {
            destination_marker.setVisible(state);
        }
        if (!state) {
            let origin_info = originInfoWindows.find(i => i.id == id);
            if (origin_info) {
                origin_info.close();
            }

            let destination_info = destinationInfoWindows.find(i => i.id == id);
            if (destination_info) {
                destination_info.close();
            }
        }

        return true;

    }
    return false;
}

function showTrackingMarker(id) {// Şoför Takip marker'ini göster   
    if (id && id > 0) {
        let marker = chauffeurMmarkers.find(i => i.id == id);
        if (marker && marker.id > 0) {
            marker.setVisible(true);
        }
    }
}
function hideTrackingMarker(id) {// Şoför Takip marker'ini gizle    
    if (id && id > 0) {
        let marker = chauffeurMmarkers.find(i => i.id == id);
        if (marker && marker.id > 0) {
            marker.setVisible(false);
            hideTrackLine(id);
            let info = chauffeurInfoWindows.find(i => i.id == id);
            if (info) {
                info.close();
            }

            reloadChauffeurData();
        }
    }
}
function toggleTrackLine(id) {// Şoför Takip çizgisini göster/ gizle
    if (id && id > 0) {
        let line = lines.find(i => i.id == id);
        if (line && line.id > 0) {
            line.setVisible(!line.getVisible());
        }
    }
}
function showTrackLine(id) {// Şoför Takip çizgisini göster
    if (id && id > 0) {
        let line = lines.find(i => i.id == id);
        if (line && line.id > 0) {
            line.setVisible(true);
        }
    }
}
function hideTrackLine(id) {// Şoför Takip çizgisini göster
    if (id && id > 0) {
        let line = lines.find(i => i.id == id);
        if (line && line.id > 0) {
            line.setVisible(false);
        }
    }
}

function getRouteOriginInfoWIndowContent(reservation) {// Route'nin Başlangıç InfoWindow Yapısı
    let str = ``;
    if (reservation && reservation.id && reservation.customer) {
        str = `<h5 class="text-center">#${reservation?.id} Başlangıç Noktası </h5>
        <ul class="nav nav-tabs" id="originTab" role="tablist">
        <li class="nav-item">
            <a class="nav-link active" id="origin-home-tab${reservation.id}" data-toggle="tab" href="#origin-home${reservation.id}" role="tab" aria-controls="origin-home${reservation.id}" aria-selected="true">Home</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="origin-profile-tab${reservation.id}" data-toggle="tab" href="#origin-profile${reservation.id}" role="tab" aria-controls="origin-profile${reservation.id}" aria-selected="false">Güzergah</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="origin-setting-tab${reservation.id}" data-toggle="tab" href="#origin-setting${reservation.id}" role="tab" aria-controls="origin-setting${reservation.id}" aria-selected="false">Ayarlar</a>
        </li>
        </ul>
        <div class="tab-content" id="routeOriginTabContent">
        <div class="tab-pane fade show active" id="origin-home${reservation.id}" role="tabpanel" aria-labelledby="origin-home-tab${reservation.id}">
            <ul class="p-2">
            <li class="mt-1"><span class="font-weight-bold">ID:</span> #${reservation?.id}</li>
            <li class="mt-1"><span class="font-weight-bold">Oluşturma:</span> ${moment(reservation?.createAt).format('DD-MM-YYYY HH:mm:ss')}</li>
            <li class="mt-1"><span class="font-weight-bold">Müşteri:</span> ${reservation?.customer?.name} ${reservation?.customer?.surname}<a href="#" onclick="showCustomerDetails(${reservation?.id})" class="font-weight-bold">Daha Fazla</a></li>
            ${reservation?.chauffeur && reservation?.chauffeur?.id ?
                `<li class="mt-1"><span class="font-weight-bold">Şoför:</span> ${reservation?.chauffeur?.name ?? ''} ${reservation?.chauffeur?.surname ?? ''} <a href="#" onclick="showChauffeurDetails(${reservation?.id})" class="font-weight-bold">Daha Fazla</a></li>`
                :
                ''
            }
            

            <li class="mt-1"><span class="font-weight-bold">Ödeme ID:</span> ${reservation.paymentId ?? ''}</li>
            <li class="mt-1"><span class="font-weight-bold">Durumu:</span> ${getReservationState(reservation?.state) ?? ''}</li>
            </ul>
        </div>
        <div class="tab-pane fade" id="origin-profile${reservation.id}" role="tabpanel" aria-labelledby="origin-profile-tab${reservation.id}">
            <ul class="p-2">
                <li class="mt-1"><span class="font-weight-bold">Baş Koord:</span> ${JSON.stringify(reservation?.origin) ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Baş Adresi:</span> ${reservation?.originAddress ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Bitiş Koord:</span> ${JSON.stringify(reservation?.destination) ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Bitiş.Adresi:</span> ${reservation?.destinationAddress ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Total:</span> ${reservation?.totalPrice ?? ''}TL</li>
                <li class="mt-1"><span class="font-weight-bold">Mesafe:</span> ${reservation?.distance ?? ''}</li>
                ${[Sofor_atandi, Varis_noktasina_gidiliyor].includes(reservation?.state) ?
                `<li class="mt-1"><span class="font-weight-bold">Şoför Lokasyon:</span> ${reservation?.chauffeur?.location?.name ?? ''}</li>`
                :
                ''
                }
                <li class="mt-1"><span class="font-weight-bold">Müşteri Lokasyon:</span> ${reservation?.customer?.location?.name ?? ''}</li>
            </ul>
        </div>
        <div class="tab-pane fade" id="origin-setting${reservation.id}" role="tabpanel" aria-labelledby="origin-setting-tab${reservation.id}">
        <ul class="p-2">
            <li class="mt-1"><span class="font-weight-bold">Markeri Ayarla: <a href="#" onclick="openEditOriginMarkerModal(${reservation?.id})" class="text-primary">Değiştir</a></li>
            <li>===================</li>
            <li class="mt-1"><span class="font-weight-bold">Güzergahı Ayarla: <a href="#" onclick="openEditRouteLineModal(${reservation?.id})" class="text-primary">Değiştir</a></li>
            <li>===================</li>
            <li class="mt-1"><span class="font-weight-bold">Güzergahı Gizle: <a href="#" onclick="hideRouteLineAndMarkers(${reservation?.id})" class="text-primary">Tıkla</a></li>
         </ul>
        </div>
        </div>
      `;
    }

    return str;
}
function getOriginMarkerTitle(reservation) {
    let origin_title = "";
    if (reservation) {
        origin_title = `
        =========== Başlangıç Noktası ===========
        #ID: ${reservation.id} - Durumu: ${getReservationState(reservation.state) ?? ''}
        Müşteri : ${reservation.customer && reservation.customer.name ? reservation.customer.name : ''} ${reservation.customer && reservation.customer.surname ? reservation.customer.surname : ''} -
        Şoför: ${reservation.chauffeur && reservation.chauffeur.name ? reservation.chauffeur.name : ''} ${reservation.chauffeur && reservation.chauffeur.surname ? reservation.chauffeur.surname : ''}
        Tarih: ${moment(reservation?.createAt).format('DD-MM-YYYY HH:mm:ss') ?? ''}
        `;
    }
    return origin_title;
};
function getRouteDestinationInfoWIndowContent(reservation) {// Route'nin Bitiş InfoWindow Yapısı    
    let str = ``;
    if (reservation && reservation.id && reservation.customer) {
        str = `<h5 class="text-center">#${reservation?.id} Bitiş Noktası </h5>
        <ul class="nav nav-tabs" id="destinationTab" role="tablist">
        <li class="nav-item">
            <a class="nav-link active" id="destination-home-tab${reservation.id}" data-toggle="tab" href="#destination-home${reservation.id}" role="tab" aria-controls="destination-home${reservation.id}" aria-selected="true">Home</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="destination-profile-tab${reservation.id}" data-toggle="tab" href="#destination-profile${reservation.id}" role="tab" aria-controls="destination-profile${reservation.id}" aria-selected="false">Güzergah</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="destination-setting-tab${reservation.id}" data-toggle="tab" href="#destination-setting${reservation.id}" role="tab" aria-controls="destination-setting${reservation.id}" aria-selected="false">Ayarlar</a>
        </li>
        </ul>
        <div class="tab-content" id="routeDestinationTabContent">
        <div class="tab-pane fade show active" id="destination-home${reservation.id}" role="tabpanel" aria-labelledby="destination-home-tab${reservation.id}">
            <ul class="p-2">
            <li class="mt-1"><span class="font-weight-bold">ID:</span> #${reservation?.id}</li>
            <li class="mt-1"><span class="font-weight-bold">Oluşturma:</span> ${moment(reservation?.createAt).format('DD-MM-YYYY HH:mm:ss') ?? ''}</li>
            <li class="mt-1"><span class="font-weight-bold">Müşteri:</span> ${reservation?.customer?.name} ${reservation?.customer?.surname}<a href="#" onclick="showCustomerDetails(${reservation?.id})" class="font-weight-bold">Daha Fazla</a></li>
            ${reservation?.chauffeur && reservation?.chauffeur?.id ?
                `<li class="mt-1"><span class="font-weight-bold">Şoför:</span> ${reservation?.chauffeur?.name ?? ''} ${reservation?.chauffeur?.surname ?? ''} <a href="#" onclick="showChauffeurDetails(${reservation?.id})" class="font-weight-bold">Daha Fazla</a></li>`
                :
                ''
            }
            <li class="mt-1"><span class="font-weight-bold">Ödeme ID:</span> ${reservation?.paymentId ?? ''}</li>
            <li class="mt-1"><span class="font-weight-bold">Durumu:</span> ${getReservationState(reservation?.state) ?? ''}</li>
            </ul>
        </div>
        <div class="tab-pane fade" id="destination-profile${reservation.id}" role="tabpanel" aria-labelledby="destination-profile-tab${reservation.id}">
            <ul class="p-2">
                <li class="mt-1"><span class="font-weight-bold">Baş Koord:</span> ${JSON.stringify(reservation?.origin) ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Baş Adresi:</span> ${reservation?.originAddress ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Bitiş Koord:</span> ${JSON.stringify(reservation?.destination) ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Bitiş.Adresi:</span> ${reservation?.destinationAddress ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Total:</span> ${reservation?.totalPrice ?? ''}TL</li>
                <li class="mt-1"><span class="font-weight-bold">Mesafe:</span> ${reservation?.distance ?? ''}</li>
                ${[Sofor_atandi, Varis_noktasina_gidiliyor].includes(reservation?.state) ?
                `<li class="mt-1"><span class="font-weight-bold">Şoför Lokasyon:</span> ${reservation?.chauffeur?.location?.name ?? ''}</li>`
                :
                ''
                }
                
                <li class="mt-1"><span class="font-weight-bold">Müşteri Lokasyon:</span> ${reservation?.customer?.location?.name ?? ''}</li>
            </ul>
        </div>
        <div class="tab-pane fade" id="destination-setting${reservation.id}" role="tabpanel" aria-labelledby="destination-setting-tab${reservation.id}">
        <ul class="p-2">
            <li class="mt-1"><span class="font-weight-bold">Markeri Ayarla: <a href="#" onclick="openEditDestinationMarkerModal(${reservation?.id})" class="text-primary">Değiştir</a></li>
            <li>===================</li>
            <li class="mt-1"><span class="font-weight-bold">Güzergahı Ayarla: <a href="#" onclick="openEditRouteLineModal(${reservation?.id})" class="text-primary">Değiştir</a></li>
            <li>===================</li>
            <li class="mt-1"><span class="font-weight-bold">Güzergahı Gizle: <a href="#" onclick="hideRouteLineAndMarkers(${reservation?.id})" class="text-primary">Tıkla</a></li>
         </ul>
        </div>
        </div>
      `;
    }
    return str;
}
function getDestinationMarkerTitle(reservation) {
    let destination_title = "";
    if (reservation) {
        destination_title = `
        =========== Bitiş Noktası ===========
        #ID: ${reservation.id} - Durumu: ${getReservationState(reservation.state) ?? ''}
        Müşteri : ${reservation.customer && reservation.customer.name ? reservation.customer.name : ''} ${reservation.customer && reservation.customer.surname ? reservation.customer.surname : ''} -
        Şoför: ${reservation.chauffeur && reservation.chauffeur.name ? reservation.chauffeur.name : ''} ${reservation.chauffeur && reservation.chauffeur.surname ? reservation.chauffeur.surname : ''}
        Tarih: ${moment(reservation?.createAt).format('DD-MM-YYYY HH:mm:ss') ?? ''}
        `;
    }
    return destination_title;
};
function getChauffeurInfoWindowCotent(chauffeur) {// Şoför InfoWindow Yapısı
    let str = ``;
    if (chauffeur && chauffeur.id) {
        str = `        
        <h5 class="text-center">Şofor: ${chauffeur?.name} ${chauffeur?.surname}</h5>        
        <ul class="nav nav-tabs" id="chauffeur-Tab" role="tablist">
        <li class="nav-item">
            <a class="nav-link active" id="chauffeur-home-tab${chauffeur.id}" data-toggle="tab" href="#chauffeur-home${chauffeur.id}" role="tab" aria-controls="chauffeur-home${chauffeur.id}" aria-selected="true">Home</a>
        </li>
        
        <li class="nav-item">
            <a class="nav-link" id="chauffeur-setting-tab${chauffeur.id}" data-toggle="tab" href="#chauffeur-setting${chauffeur.id}" role="tab" aria-controls="chauffeur-setting${chauffeur.id}" aria-selected="false">Ayarlar</a>
        </li>
        </ul>
        <div class="tab-content" id="chauffeurTabContent">
            <div class="tab-pane fade show active" id="chauffeur-home${chauffeur.id}" role="tabpanel" aria-labelledby="chauffeur-home-tab${chauffeur.id}">
            <ul class="p-2">
                <li class="mt-1"><span class="font-weight-bold">#ID:</span> #${chauffeur?.id}</li>
                <li class="mt-1"><span class="font-weight-bold">Durumu:</span> ${getChauffeurState(chauffeur?.chauffeurState) ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Kullanıcı adı:</span> ${chauffeur?.username ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Tel No:</span> ${chauffeur?.phone ?? ''}</li>
                <li class="mt-1"><span class="font-weight-bold">Email:</span> ${chauffeur?.email}</li>
                <li class="mt-1"><span class="font-weight-bold">Adres:</span> ${chauffeur?.address ?? ''}</li>
            </ul>
            </div>    
            <div class="tab-pane fade" id="chauffeur-setting${chauffeur.id}" role="tabpanel" aria-labelledby="chauffeur-setting-tab${chauffeur.id}">
            <ul class="p-2">
                <li class="mt-1"><span class="font-weight-bold">Takip Çizgisini Gizle / Göster:</span><a href="#" onclick="toggleTrackLine(${chauffeur?.id})" class="text-primary"> Değiştir</a></li>
                <li class="mt-1"><span class="font-weight-bold">Takip Çizgisini Ayarla </span><a href="#" onclick="openEditChauffeurLineModal(${chauffeur?.id})" class="text-primary"> Değiştir</a></li>
                <li class="mt-1"><span class="font-weight-bold">Takibi Bırak:</span><a href="#" onclick="hideTrackingMarker(${chauffeur?.id})" class="text-primary"> Değiştir</a></li>
                <li class="mt-1"><span class="font-weight-bold">Takibi Çizgisini Sınırla:</span><a href="#" onclick="openEditChauffeurTrackLineModal(${chauffeur?.id})" class="text-primary"> Değiştir</a></li>
            </ul>
            </div>
        </div>
      `;
    }
    return str;
}

function openEditOriginMarkerModal(id) { // Route'nin başlangıç markerini tekrar ayarlamak için açılan modal
    if (id && id > 0) {
        let marker = reservationOriginMarkers.find(i => i.id == id);
        let reservation = reservations.find(i => i.id == id);
        if (marker && reservation) {
            $('#submit_origin_marker_form input[name="Id"]').val(id);
            $('#submit_origin_marker_form input[name="Color"]').val(reservation?.originMarkerOption?.icon.color);
            $('#submit_origin_marker_form input[name="Opacity"]').val(reservation?.originMarkerOption?.icon.opacity);
            $('#origin_marker_form').modal('show');
        }
        else {
            Swal.fire('Dikkat', 'Geçersiz işlemde bulundunuz.', 'warning');
        }
    }

}
function openEditDestinationMarkerModal(id) {// Route'nin bitiş markerini tekrar ayarlamak için açılan modal
    if (id && id > 0) {
        let marker = reservationDestinationMarkers.find(i => i.id == id);
        let reservation = reservations.find(i => i.id == id);
        if (marker && reservation) {
            $('#submit_destination_marker_form input[name="Id"]').val(id);
            $('#submit_destination_marker_form input[name="Color"]').val(reservation?.destinationMarkerOption?.icon.color);
            $('#submit_destination_marker_form input[name="Opacity"]').val(reservation?.destinationMarkerOption?.icon.opacity);
            $('#destination_marker_form').modal('show');
        }
        else {
            Swal.fire('Dikkat', 'Geçersiz işlemde bulundunuz.', 'warning');
        }
    }
}
function openEditRouteLineModal(id) {// Route'nin çizgisini tekrar ayarlamak için açılan modal
    if (id && id > 0) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.directionsRenderer) {
            $('#submit_route_line_form input[name="Id"]').val(id);
            $('#submit_route_line_form input[name="Color"]').val(reservation?.lineOption?.color);
            $('#submit_route_line_form input[name="Opacity"]').val(reservation?.lineOption?.strokeOpacity);
            $('#submit_route_line_form input[name="Weight"]').val(reservation?.lineOption?.strokeWeight);
            $('#route_line_form').modal('show');
        }
        else {
            Swal.fire('Dikkat', 'Geçersiz işlemde bulundunuz.', 'warning');
        }
    }
}
function openEditChauffeurLineModal(id) {// Route'nin çizgisini tekrar ayarlamak için açılan modal
    if (id && id > 0) {
        let line = lines.find(i => i.id == id);
        if (line) {
            $('#submit_chauffeur_line_form input[name="Id"]').val(id);
            $('#submit_chauffeur_line_form input[name="Color"]').val(line?.strokeColor);
            $('#submit_chauffeur_line_form input[name="Opacity"]').val(line?.strokeOpacity);
            $('#submit_chauffeur_line_form input[name="Weight"]').val(line?.strokeWeight);
            $('#chauffeur_line_form').modal('show');
        }
        else {
            Swal.fire('Dikkat', 'Geçersiz işlemde bulundunuz.', 'warning');
        }
    }
}
function openEditChauffeurTrackLineModal(id) {// Route'nin çizgisini Zaman Bazlı Filtreleme
    if (id && id > 0) {
        let line = lines.find(i => i.id == id);
        if (line) {
            $('#submit_chauffeur_line_filter input[name="Id"]').val(id);
            $('#submit_chauffeur_line_filter input[name="Minut"]').val(0);
            $('#chauffeur_line_filter').modal('show');
        }
        else {
            Swal.fire('Dikkat', 'Geçersiz işlemde bulundunuz.', 'warning');
        }
    }
}

$(function () {
    // =================== Origin marker ===================
    $('#origin_marker_form button[type="button"]').click(() => {
        $('#submit_origin_marker_form input[name="Color"]').val(null);
        $('#submit_origin_marker_form input[name="Opacity"]').val(null);
    });

    $('#submit_origin_marker_form').submit((e) => {
        e.preventDefault();
        let form = $('#submit_origin_marker_form');
        let data = new FormData(form[0]);
        if (data.get("Id") && data.get("Color") && data.get("Opacity")) {
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
                    let marker = reservationOriginMarkers.find(i => i.id == id);
                    let option = {
                        icon: {
                            color: data.get("Color"),
                            opacity: data.get("Opacity")
                        }

                    };
                    editRouteOriginMarker(marker, option);
                    $('#origin_marker_form').modal('hide');
                    Swal.close()
                }
            })
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
        }
    });
    // =================== Destination marker ===================
    $('#destination_marker_form button[type="button"]').click(() => {
        $('#submit_destination_marker_form input[name="Color"]').val(null);
        $('#submit_destination_marker_form input[name="Opacity"]').val(null);
    });

    $('#submit_destination_marker_form').submit((e) => {
        e.preventDefault();
        let form = $('#submit_destination_marker_form');
        let data = new FormData(form[0]);
        if (data.get("Id") && data.get("Color") && data.get("Opacity")) {
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
                    let marker = reservationDestinationMarkers.find(i => i.id == id);
                    let option = {
                        icon: {
                            color: data.get("Color"),
                            opacity: data.get("Opacity")
                        }
                    };
                    editRouteDestinationMarker(marker, option);
                    $('#destination_marker_form').modal('hide');
                    Swal.close()
                }
            })
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
        }
    });

    // =================== Route line ===================
    $('#route_line_form button[type="button"]').click(() => {
        $('#submit_route_line_form input[name="Color"]').val(null);
        $('#submit_route_line_form input[name="Opacity"]').val(null);
        $('#submit_route_line_form input[name="Weight"]').val(null);
    });

    $('#submit_route_line_form').submit((e) => {
        e.preventDefault();
        let form = $('#submit_route_line_form');
        let data = new FormData(form[0]);
        if (data.get("Id") && data.get("Color") && data.get("Opacity") && data.get("Weight")) {
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
                    let option = {
                        color: data.get("Color"),
                        strokeOpacity: data.get("Opacity"),
                        strokeWeight: data.get("Weight"),
                    };
                    let reservation = reservations.find(i => i.id == id);
                    if (reservation && reservation.directionsRenderer) {
                        changeRouteLine(reservation.directionsRenderer, option);
                    }
                    $('#route_line_form').modal('hide');
                    Swal.close()
                }
            })
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
        }
    });

    // =================== Chauffeur Track line ===================
    $('#chauffeur_line_form button[type="button"]').click(() => {
        $('#submit_chauffeur_line_form input[name="Color"]').val(null);
        $('#submit_chauffeur_line_form input[name="Opacity"]').val(null);
        $('#submit_chauffeur_line_form input[name="Weight"]').val(null);
    });

    $('#submit_chauffeur_line_form').submit((e) => {
        e.preventDefault();
        let form = $('#submit_chauffeur_line_form');
        let data = new FormData(form[0]);
        if (data.get("Id") && data.get("Color") && data.get("Opacity") && data.get("Weight")) {
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

                    let line = lines.find(i => i.id == id);
                    if (line) {
                        line.setOptions({
                            strokeColor: data.get("Color"),
                            strokeOpacity: parseFloat(data.get("Opacity")),
                            strokeWeight: parseFloat(data.get("Weight"))
                        });
                    }
                    $('#chauffeur_line_form').modal('hide');
                    Swal.close()
                }
            })
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
        }
    });

    // =================== Chauffeur Track line Filter ===================
    $('#chauffeur_line_filter button[type="button"]').click(() => {
        $('#submit_chauffeur_line_filter input[name="Minut"]').val(0);
    });

    $('#submit_chauffeur_line_filter').submit((e) => {
        e.preventDefault();
        let form = $('#submit_chauffeur_line_filter');
        let data = new FormData(form[0]);
        if (data.get("Id") && data.get("Minut")) {
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
            let minut = data.get("Minut");
            let tmp_data = {
                chauffeurId: id,
                minut: minut
            };
            axios({
                url: '/Tracking/FilterTrackLine',
                method: 'Post',
                data: tmp_data,
                headers: { 'Content-Type': 'application/json' }
            })
                .then(res => {
                    if (res && res.data && res.data.isSuccess) {
                        Swal.fire("DİKKAT", res.data.message, 'success');
                        res.data.data?.forEach(i => {
                            i.lat = parseFloat(i.lat);
                            i.lng = parseFloat(i.lng);
                        });
                        let chauffeur = chauffeurs.find(i => i.id == id);
                        if (chauffeur) {
                            chauffeur.directionList = res.data.data;
                        }

                        let line = lines.find(i => i.id == id);
                        if (line) {
                            line.setPath(res.data.data);
                        }
                    }
                    else {
                        Swal.fire("UYARI", res.data.message, 'warning');
                    }
                })
                .catch(err => {
                    console.log("err ::::: ", err);
                    Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
                }).finally(() => {
                    $('#chauffeur_line_filter').modal('hide');
                });
        }
        else {
            Swal.fire("Geçersiz işlemde bulundunuz", '', 'warning');
        }
    });
});

function showChauffeurDetails(id) {
    if (id) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.chauffeur) {
            let chauffeur = reservation.chauffeur;
            let title = `<strong class="text-center text-primary">${chauffeur?.name} ${chauffeur?.surname}</strong>`;
            let html = `
            <div class="p-0 m0">
                <ul class="text-left">
                  <li><strong>#ID:</strong> #${chauffeur?.id}</li>
                  <li ${[Tamamlandi, Iptal_edildi, Iptal_isteği_atildi].includes(reservation.state) ? 'class="d-none"' : ''}><strong>Şoför Durumu:</strong> ${getChauffeurState(chauffeur?.chaufferState) ?? ''}</li>
                  <li><strong>Şoför Lokasyonu:</strong> ${chauffeur?.location?.name}</li>
                  <li><strong>Tel No:</strong> ${chauffeur?.phone ?? ''}</li>
                  <li><strong>Email:</strong> ${chauffeur?.email ?? ''}</li>
                  <li><strong>Adres:</strong> ${chauffeur?.address ?? ''}</li>
                  <li><strong>Kayıt Tarihi:</strong> ${moment(chauffeur?.createAt).format('DD-MM-YYYY HH:mm:ss') ?? ''}</li>
                </ul>
              </div>
            `;
            Swal.fire({
                title: title,
                icon: 'info',
                html: html,
                confirmButtonText: 'Kapat',
                showClass: {
                    popup: 'animate__animated animate__fadeInDown'
                },
                hideClass: {
                    popup: 'animate__animated animate__fadeOutUp'
                }
            })
        }

    }
}
function showCustomerDetails(id) {
    if (id) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation && reservation.customer) {
            let customer = reservation.customer;
            let title = `<strong class="text-center text-primary">${customer?.name} ${customer?.surname}</strong>`;
            let html = `
            <div class="p-0 m0">
                <ul class="text-left">
                  <li><strong>#ID:</strong> #${customer?.id}</li>
                  <li><strong>Kayıtlı mı:</strong> ${customer?.nonRegister ? 'Hayır' : 'Evet'} </li>
                  <li><strong>Müşteri Lokasyonu:</strong> ${customer?.location?.name ?? ''}</li>
                  <li><strong>Ülke kısaltması:</strong> ${customer?.cca2 ?? ''}</li>
                  <li><strong>Ülke Kodu:</strong> ${customer?.callingCode ?? ''}</li>
                  <li><strong>Tel No:</strong> ${customer?.phone ?? ''}</li>        
                  <li><strong>Email:</strong> ${customer?.email ?? ''}</li>
                  <li><strong>kayıt Adresi:</strong> ${customer?.address ?? ''}</li>
                  <li><strong>Kayıt Tarihi:</strong> ${moment(customer?.createAt).format('DD-MM-YYYY HH:mm:ss') ?? ''}</li>
               
                </ul>
              </div>
            `;
            Swal.fire({
                title: title,
                icon: 'info',
                html: html,
                confirmButtonText: 'Kapat',
                showClass: {
                    popup: 'animate__animated animate__fadeInDown'
                },
                hideClass: {
                    popup: 'animate__animated animate__fadeOutUp'
                }
            })
        }

    }
}

function getChauffeurState(state_id) {
    let state = "Tespit Edilemedi. ";
    if (state_id == Sofor_bos) state = "Boş";
    else if (state_id == Yolad_musteriye_gidiliyor) state = "Yolda, müşteriye gidiliyor.";
    else if (state_id == Musteri_alindi_varis_noktasina_gidiliyor) state = "Müşteri teslim alındı, varış noktasına gidiliyor.";
    else if (state_id == Sofor_bulusma_noktasına_vardi_bekliyor) state = "Soför buluşma noktasına vardı bekliyor";
    return state;
}
function getReservationState(state_id) {
    let state = "Tespit Edilemedi.";
    if (state_id == Odeme_yapildi_bekleniyor) state = "Ödeme yapıldı bekleniyor";
    else if (state_id == Odeme_islemi_basarisiz) state = "Ödeme işlemi başarısız.";
    else if (state_id == Sofor_atandi) state = "Şoför atandı.";
    else if (state_id == Varis_noktasina_gidiliyor) state = "Müşteri alındı eve gidiliyor.";
    else if (state_id == Tamamlandi) state = " Tamamlandı.";
    else if (state_id == Iptal_edildi) state = " İptal edildi.";
    else if (state_id == Iptal_isteği_atildi) state = " İptal isteği atıldı.";
    else if (state_id == Tamamlanmadi) state = " Tamamlanmadı.";
    return state;
}

function AddNewReservation(reservation) {
    reservation.locationId = reservation.customer.locationId; //Rezervasyonlarda müşteri baz alınarak lokasyon atanıyor
    reservation.directionsRenderer = null;//calculateAndDisplayRoute(reservation);
    reservation.lineOption = defaultRouteLineOption;
    reservation.originMarkerOption = {
        title: getOriginMarkerTitle(reservation),
        icon: defaultOriginMarkerIconOption,
    };
    reservation.destinationMarkerOption = {
        title: getDestinationMarkerTitle(reservation),
        icon: defaultDestinationMarkerIconOption,

    };
    reservations.push(reservation);
}
function refreshReservationInfoWindowContentAndTitile(id) {//Rezervasyon durumu güncellendiğinde, haritada ona bağlı info content ve marker title güncellemesi
    if (id && id > 0) {
        let reservation = reservations.find(i => i.id == id);
        if (reservation) {
            let origin_marker = reservationOriginMarkers.find(i => i.id == id);
            if (origin_marker) {
                let str = getOriginMarkerTitle(reservation);
                origin_marker.setTitle(str);
                reservation.originMarkerOption.title = str;
            }
            let destination_marker = reservationDestinationMarkers.find(i => i.id == id);
            if (destination_marker) {
                let str = getDestinationMarkerTitle(reservation);
                destination_marker.setTitle(str);
                reservation.destinationMarkerOption.title = str;
            }
            let origin_marker_info = originInfoWindows.find(i => i.id == id);
            if (origin_marker_info) {
                origin_marker_info.setContent(getRouteOriginInfoWIndowContent(reservation));
            }
            let destination_marker_info = destinationInfoWindows.find(i => i.id == id);
            if (destination_marker_info) {
                destination_marker_info.setContent(getRouteDestinationInfoWIndowContent(reservation));
            }
        }

    }

}
function refreshChauffeurInfoWindowCotent(id) {// Şoför durumu güncellendiğinde, haritada ona bağlı info content ve marker title güncellemesi
    if (id && id > 0) {
        let chauffeur = chauffeurs.find(i => i.id == id);
        if (chauffeur) {
            let marker = chauffeurMmarkers.find(i => i.id == id);
            if (marker) {
                let title = `${chauffeur?.name} ${chauffeur?.surname} - Durumu: ${getChauffeurState(chauffeur?.chauffeurState)}`;
                marker.setTitle(title);
            }
            let info_window = chauffeurInfoWindows.find(i => i.id == id);
            if (info_window) {
                let content = getChauffeurInfoWindowCotent(chauffeur);
                info_window.setContent(content);
            }

        }

    }

}
function refreshTrackLine(id, location, directionList) {//şoför takip işlemi gelen son lokasyon bilgisine göre takip çizgisini ve markerini güncelleme 
    if (id && location && location.lat && location.lng && directionList && directionList.length > 0) {
        let marker = chauffeurMmarkers.find(i => i.id == id);
        if (marker) {
            marker.setPosition(location);
        }
        let line = lines.find(i => i.id == id);
        if (line) {
            line.setPath(directionList);
        }
    }
}


