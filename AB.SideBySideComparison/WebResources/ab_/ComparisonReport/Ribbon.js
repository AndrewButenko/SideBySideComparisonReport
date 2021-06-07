var AB = AB || {};
AB.ComparisonReport = (function () {

    function openReport(entityType, recordIds) {
        var data = {
            entityType: entityType,
            Id1: recordIds[0],
            Id2: recordIds[1]
        };

        if (Xrm.Navigation && Xrm.Navigation.navigateTo) {
            Xrm.Navigation.navigateTo({
                    pageType: "webresource",
                    webresourceName: "ab_/ComparisonReport/index.html",
                    data: JSON.stringify(data)
                },
                {
                    target: 2,
                    position: 1,
                    width: { value: 80, unit: "%" },
                    height: { value: 80, unit: "%" },
                    title: "Comparison Report"
                });
        } else {
            var url = '/WebResources/ab_/ComparisonReport/index.html?data=' + JSON.stringify(data);
            window.location.open(url);
        }
    }

    return {
        openReport: openReport
    };
})();