// Пелена во время загрузок
function AjaxLoader(selector) {
    this.Counter = 0; // using counter
    this.Ajaxloader = $(selector); // sheet

    $(window).ajaxStart(function () {
        AjaxLoader.Show();
    });

    $(window).ajaxStop(function () {
        AjaxLoader.Dispose();
    });

    $(window).ajaxError(function (event, request, settings) {
        AjaxLoader.Dispose();
        //alert(request.responseText);
        document.write(request.responseText);
    });
};

// Show sheet
AjaxLoader.prototype.Show = function () {
    var _this = this;
    _this.Counter++;
    if (_this.Counter < 2) {
        _this.Ajaxloader
            .stop(true, true)
            .animate({
                top: "4px"
            }, "slow");
    }
};

// Dispose sheet
AjaxLoader.prototype.Dispose = function () {
    var _this = this;
    _this.Counter--;
    if (_this.Counter < 0)
        _this.Counter = 0;

    if (_this.Counter == 0) {
        _this.Ajaxloader
            .stop()
            .animate({
                top: "-24px"
            }, "slow");
    }

    return _this.Counter != 0;
};

$(function () {
    AjaxLoader = new AjaxLoader("#ajaxloader");
})