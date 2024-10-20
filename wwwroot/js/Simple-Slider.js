document.addEventListener("DOMContentLoaded", function() {
    var mySwiper = new Swiper('.swiper-container', {
        loop: true,  // Enable looping
        autoplay: {
            delay: 3000,  // Auto-scroll every 3 seconds
            disableOnInteraction: false,  // Allow autoplay to continue after user interaction
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true
        }
    });
});
