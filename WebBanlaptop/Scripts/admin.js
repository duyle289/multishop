////const { EALREADY } = require("constants");
////const { type } = require("jquery");

var admin = {
    init: function () {
        admin.event();
    },
    event: function () {

        //================================
        // modal khóa tải khoản admin
        $('.modal-lock-admin').off('click').on('click', function () {
            debugger;
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-lock-admin').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlockAdmin(id);
        });

        //================================
        // modal khóa bình luận
        $('.modal-change-bl').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-change-bl').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlockBL(id);
        });
        // modal trạng thái san pham con hang hay het hang
        $('.modal-change-sp').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-change-sp').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlockSP(id);
        });
        // modal trạng thái tin tức đã đăng hay chưa đăng
        $('.modal-change-tt').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-change-tt').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlockTT(id);
        });
        // modal trạng thai loai san phâm
        $('.modal-lock-lsp').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-lock-lsp').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlocklsp(id);
        });
        // modal trạng thai còn hơp tác hay het hop tac voi nha san xuat
        $('.modal-change-nsx').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-change-nsx').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            admin.lockOrUnlockNSX(id);
        });
    },

    lockOrUnlockAdmin: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockAdmin',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Khóa tài khoản thành công');
                    else alert('mở khóa tài khoản thành công');
                    $('#lockAccount').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    },
    lockOrUnlockBL: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockBL',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Khóa bình luận thành công');
                    else alert('Mở bình luận thành công');
                    $('#changeTTBL').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    },
    lockOrUnlockSP: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockSP',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Hết hàng');
                    else alert('Còn hàng');
                    $('#changeTTSP').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    },
    lockOrUnlockTT: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockTT',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Chưa đăng tin tức');
                    else alert('Đã đăng tin tức');
                    $('#changeTTTT').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    },
    lockOrUnlocklsp: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlocklsp',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Không tồn tại');
                    else alert('Tồn tại');
                    $('#lockLsp').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    },
    lockOrUnlockNSX: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockNSX',
            data: {
                id: id
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response.status) { // trả về giá trị khóa tài khoản thành công
                    if (response.lockStatus) alert('Hết hợp tác');
                    else alert('Còn hợp tác');
                    $('#changeNSX').modal('hide');
                    location.reload(); // reload lại trang
                } else { // khóa lỗi do người bị khóa là chủ sở hữu
                    alert(response.exit);
                }
            },
            error: function (err) {
                console.log(err);
                alert(err.errorMessage)
            }
        });
    }

};

admin.init();