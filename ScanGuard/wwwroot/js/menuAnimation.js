$(function () {
    var str = '#len';
    $(document).ready(function () {
        var i = 1, stop = 14; // ���������� ��������� ����
        setInterval(function () {
            if (i > stop) {
                return;
            }
            $('#len' + (i++)).toggleClass('bounce');
        }, 500);
    });
});