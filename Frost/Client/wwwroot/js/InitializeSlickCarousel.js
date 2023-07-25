window.initializeSlickCarousel = function () {
    $('.image-slider').not('.slick-initialized').slick({
        infinite: true,
        swipe: false
    })
};
window.removeSlide = function (index) {
    console.log('hejka');
    $('.image-slider').slick('slickRemove', index,false);
};
window.unslick = function () {
    $('.image-slider').slick('unslick');
}
window.getCurrentSlide = function () {
    var currentSlide = $('.image-slider').slick('slickCurrentSlide');
    return currentSlide;
}