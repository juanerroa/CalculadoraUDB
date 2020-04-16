(function($) {
  'use strict';
  $(function() {
    // $('#sidebar .nav').perfectScrollbar();
    $('.container-scroller').perfectScrollbar( {suppressScrollX: true});
  });

  $(".form-check label,.form-radio label").append('<i class="input-helper"></i>');
})(jQuery);
