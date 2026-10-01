document.getElementById("year").textContent = new Date().getFullYear();


// Sticky Navbar
$(window).scroll(function () {
    if ($(this).scrollTop() > 0) {
        $('.ct-header').addClass('ct-header-shadow');
    } else {
        $('.ct-header').removeClass('ct-header-shadow');
    }
});



