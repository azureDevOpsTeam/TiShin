
;(function(){
    // Prevent redeclaration across multiple loads
    window.defaultCarouselSwiper = window.defaultCarouselSwiper || new Swiper(".default-carousel", {
        loop: true,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
    });

    var swiper = window.defaultCarouselSwiper;

    var customSwiperNext = document.querySelector('.custom-swiper-next');
    var customSwiperPrev = document.querySelector('.custom-swiper-prev');

    if (customSwiperNext) {
        customSwiperNext.addEventListener('click', function(){
            swiper.slideNext();
        });
    }

    if (customSwiperPrev) {
        customSwiperPrev.addEventListener('click', function(){
            swiper.slidePrev();
        });
    }
})();



//####################################################################

;(function(){ new Swiper(".amazing-carousel", {
    slidesPerView: "auto",
    spaceBetween: 10,
    freeMode: true,
}); })();

;(function(){ new Swiper(".landing-amazing-carousel", {
    slidesPerView: 2,
    spaceBetween: 10,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    breakpoints: {
        100: { slidesPerView: 1 },
        576: { slidesPerView: 2 },
        768: { slidesPerView: 3 },
        1024: { slidesPerView: 4 },
    },
}); })();

;(function(){ new Swiper(".category-carousel", {
    slidesPerView: 5,
    spaceBetween: 30,
    pagination: {
        el: ".swiper-pagination",
        clickable: true,
    },
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
}); })();

;(function(){ new Swiper(".product-carousel", {
    slidesPerView: 5,
    spaceBetween: 10,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    breakpoints: {
        100: { slidesPerView: 1 },
        576: { slidesPerView: 2 },
        768: { slidesPerView: 3 },
        1024: { slidesPerView: 4 },
        1400: { slidesPerView: 5 }
    },
}); })();

;(function(){ new Swiper(".product-list-carousel", {
    slidesPerView: 5,
    spaceBetween: 10,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    breakpoints: {
        100: { slidesPerView: 1 },
        576: { slidesPerView: 2 },
        1200: { slidesPerView: 4 },
    },
}); })();


;(function(){ new Swiper(".blog-carousel", {
    slidesPerView: 5,
    spaceBetween: 10,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    breakpoints: {
        100: { slidesPerView: 1 },
        576: { slidesPerView: 2 },
        992: { slidesPerView: 3 },
        1200: { slidesPerView: 4 },
    },
}); })();


;(function(){ var swiperProductGalleryOne= new Swiper("#productGalleryOne", {
    spaceBetween: 10,
    slidesPerView: 3,
    freeMode: true,
    watchSlidesProgress: true,
});
var swiperProductGalleryTwo=new Swiper("#productGalleryTwo", {
    spaceBetween: 10,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    thumbs: {
        swiper: swiperProductGalleryOne,
    },
}); })();


;(function(){ new Swiper(".free-mode", {
    slidesPerView: "auto",
    spaceBetween: 10,
    freeMode: true,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
}); })();
