(function($) {
  'use strict';
  $(function() {
    $('#order-listing').DataTable({
      "aLengthMenu": [[5, 10, 15, -1], [5, 10, 15, "Hepsi"]],
        "iDisplayLength": 10,
        "responsive": true,
        "lengthChange": true,
        "autoWidth": false,
        "buttons": ['excel', 'pdf', 'print'],
        "language": {
            "buttons": {
                "copyTitle": "Panoya kopyalandý.",
                "copySuccess": "Panoya %d satýr kopyalandý",
                "copy": "Kopyala",
                "print": "Yazdýr",
            },
            "paginate": { "next": "Sonraki", "previous": "Onceki" },
            "info": "Suan _PAGE_. sayfadasiniz. Toplam _PAGES_ adet sayfa var.",
            "lengthMenu": " _MENU_ Listede kac adet gozuksun?",
            "search": "Arama Yap",
            "emptyTable": "Tabloda herhangi bir veri mevcut degil",
            "Processing": "Bekleyiniz...",
            "zeroRecords":"Kayit bulunamadi!"

        },

    });

    $('#order-listing').each(function(){
      var datatable = $(this);
      // SEARCH - Add the placeholder for Search and Turn this into in-line form control
      var search_input = datatable.closest('.dataTables_wrapper').find('div[id$=_filter] input');
      search_input.attr('placeholder', 'Ara');
      search_input.removeClass('form-control-sm');
      // LENGTH - Inline-Form control
      var length_sel = datatable.closest('.dataTables_wrapper').find('div[id$=_length] select');
      length_sel.removeClass('form-control-sm');
    });
  });
})(jQuery);
