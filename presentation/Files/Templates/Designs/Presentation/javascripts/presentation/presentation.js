(function($, undefined) {
	$('#impress').prepend('<section class="slide-welcome"></section>');

	$('#impress > *').addClass('step');

	// if (false)
	$('#impress .step').each(function(i, el) {
		$(el).attr('data-x', 1300*i);

		if (false)
		{
		if (i%2 == 1) {
			$(el).attr('data-rotate', 30*i);
		}
		if (i%3 == 1) {
			$(el).attr('data-rotate-x', 20*i);
		}
		if (i%3 == 1) {
			$(el).attr('data-z', 20*i);
		}
		}

	});
	impress().init();

	$(document).on('impress:stepenter', function(event) {
		var numberOfSlides = 0, currentSlide = 0;
		$('#impress .step').each(function(index, el) {
			if ($(el).hasClass('active')) {
				currentSlide = index+1;
			}
			numberOfSlides++;
		});
		$('#status').html(currentSlide+'/'+numberOfSlides);
	});
}(jQuery));
