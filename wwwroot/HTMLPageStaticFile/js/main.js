// Sticky Navbar
$(window).scroll(function () {
    if ($(this).scrollTop() > 200) {
        $('.sticky-top').addClass('shadow-sm').css({ top: '0px', background: "#fff" });
    } else {
        $('.sticky-top').removeClass('shadow-sm').css({ top: '0px', background: "#f6f6f6" });
    }
});