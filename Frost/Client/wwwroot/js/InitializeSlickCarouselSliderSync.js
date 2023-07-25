window.initializeSlickCarouselSliderSync = function () {
	$('.slider-for').not('.slick-initialized').slick({
		slidesToShow: 1,
		slidesToScroll: 1,
		arrows: true,
		fade: true,
		asNavFor: '.slider-nav'
	});
	$('.slider-nav').not('.slick-initialized').slick({
		slidesToShow: 5,
		asNavFor: '.slider-for',
		arrows: true,
		dots: false,
		centerMode: true,
		focusOnSelect: true,
		infinite: true,
		draggable:true
	});

};
window.unslick = function () {
	$('.slider-nav').slick('unslick');
}