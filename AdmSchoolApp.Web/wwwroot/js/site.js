window.initDataTable = (tableId) => {
    if ($.fn.DataTable.isDataTable(`#${tableId}`)) {
        $(`#${tableId}`).DataTable().destroy();
    }
    
    $(`#${tableId}`).DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/pt-BR.json'
        },
        pageLength: 10,
        responsive: true,
        order: [[0, 'desc']]
    });
};