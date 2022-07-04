////const { EALREADY } = require("constants");
////const { type } = require("jquery");

var user = {
    init: function () {
        user.event();
    },
    event: function () {
        
        //================================
        // modal khóa tải khoản admin
        $('.modal-lock-user').off('click').on('click', function () {
            var id = $(this).data('account');
            $('#hiddenAccount').val(id);
        });
        $('#btn-lock-user').off('click').on('click', function () {
            var id = $('#hiddenAccount').val();
            user.lockOrUnlockUser(id);
        });
        //================================
        
    },

    lockOrUnlockUser: function (id) {
        $.ajax({
            url: '/Admin/LockOrUnlockUser',
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
    }
};

user.init();