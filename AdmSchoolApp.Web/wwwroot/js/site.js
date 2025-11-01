window.initDataTable = (selector) => {
    try {
        if (!window.jQuery || !jQuery().DataTable) return;
        const el = document.querySelector(selector);
        if (!el) return;
        // destroy existing if present
        if ($.fn.dataTable.isDataTable(el)) {
            $(el).DataTable().destroy();
        }
        $(el).DataTable({
            paging: true,
            searching: true,
            info: true,
            lengthChange: true,
            pageLength: 10,
            order: [],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/pt-BR.json'
            }
        });
    } catch (e) {
        console.error('initDataTable error', e);
    }
};
