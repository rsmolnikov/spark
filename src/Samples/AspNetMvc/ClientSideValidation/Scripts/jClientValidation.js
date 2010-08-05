;
(function($) {
	/**
	* Converts Fluent validation to jQuery.validation
	* @alias jQuery.prototype.clientValidation
	* @param {Object} options
	* @return {Array}
	*/
	$.fn.clientValidation = function(options) {
		var o = $.extend({}, $.fn.clientValidation.defaults, options);

		return this.each(function() {
			var $this = $(this);

			o.initialize();

			// Replace date validator with better regex pattern
			$.validator.addMethod('date', function(value, element) {
				var regexp = /^(?=\d)(?:(?:(?:(?:(?:0?[13578]|1[02])(\/|-|\.)31)\1|(?:(?:0?[1,3-9]|1[0-2])(\/|-|\.)(?:29|30)\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})|(?:0?2(\/|-|\.)29\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))|(?:(?:0?[1-9])|(?:1[0-2]))(\/|-|\.)(?:0?[1-9]|1\d|2[0-8])\4(?:(?:1[6-9]|[2-9]\d)?\d{2}))($|\ (?=\d)))?(((0?[1-9]|1[012])(:[0-5]\d){0,2}(\ [AP]M))|([01]\d|2[0-3])(:[0-5]\d){1,2})?$/;
				var re = new RegExp(regexp);
				return this.optional(element) || re.test(value);
			}, 'Field does not match the specified pattern.');

			// Add Regex validator
			$.validator.addMethod('regex', function(value, element, regexp) {
				var re = new RegExp(regexp);
				return this.optional(element) || re.test(value);
			}, 'Field does not match the specified pattern.');

            // Add RangeEx validator
			$.validator.addMethod('rangeEx', function(value, element, params) {
				return this.optional(element) || ( value > params[0] && value < params[1] );
			}, 'Value is not in the specified range.');

			// Add dateRange validator
			$.validator.addMethod('dateRange', function(value, element, params) {
				var dateValue = new Date(value);
				var minDate = new Date(params[0]);
				var maxDate = new Date(params[1]);
				return this.optional(element) || minDate <= dateValue && maxDate >= dateValue;
			}, 'Date is not in the specified range.');

			// Add dateMin validator
			$.validator.addMethod('dateMin', function(value, element, param) {
				var dateValue = new Date(value);
				var minDate = new Date(param);
				return this.optional(element) || minDate <= dateValue;
			}, 'Date is less than the specified minimum.');

			// Add dateMax validator
			$.validator.addMethod('dateMax', function(value, element, param) {
				var dateValue = new Date(value);
				var maxDate = new Date(param);
				return this.optional(element) || maxDate >= dateValue;
			}, 'Date is later than specified maximum.');

			var id = $this.attr('id');
			var isValidationPresent = (typeof (clientValidation) != 'undefined');

			var isNameSpacePresent = false;
			if (isValidationPresent) {
				isNameSpacePresent = (typeof (clientValidation[id]) != 'undefined');
			}

			if (id && isNameSpacePresent) {
				if (o.isValid()) {

					var validatorRules = 'rules: {';
					var validatorMessages = 'messages: {';
					var rules = clientValidation[id]['rules'];

					for (rule in rules) {
						var field = rules[rule]['Field'];
						var validatorRule = '\'' + field + '\'' + ': {';
						var validatorMessage = '\'' + field + '\'' + ': {';
						var attributes = rules[rule]['Attributes'];

						for (attribute in attributes) {
							var ruleName = attributes[attribute].ValidationType;
							var message = attributes[attribute].ErrorMessage ? (attributes[attribute].ErrorMessage).replace(new RegExp('\'', 'g'), '\\\'') : '';

							switch (ruleName) {
								case 'required':
									validatorRule += 'required: true, ';
									validatorMessage += 'required: $.format(\'' + message + '\'), ';
									break;
								case 'stringLength':
									var min = attributes[attribute].ValidationParameters.minimumLength;
									var max = attributes[attribute].ValidationParameters.maximumLength;

									validatorRule += 'minlength: ' + min + ', ';
									validatorMessage += 'minlength: $.format(\'' + message + '\'), ';
                                    validatorRule += 'maxlength: ' + max + ', ';
                                    validatorMessage += 'maxlength: $.format(\'' + message + '\'), ';				
									break;
								case 'range':
									var min = attributes[attribute].ValidationParameters.minimum+"";//convert to string
									var max = attributes[attribute].ValidationParameters.maximum+"";//convert to string
                                    var exclusive = attributes[attribute].ValidationParameters.exclusive;
									var pattern = /^\d{4}[\/-]\d{1,2}[\/-]\d{1,2}/g;
									if (max.match(pattern) != null) {
										max = max.match(pattern)[0].replace(/-/g, "/");
										min = min.match(pattern)[0].replace(/-/g, "/");
									}
									if (!/Invalid|NaN/.test(new Date(max))) {
										min = new Date(min);
										max = new Date(max);
                                        //01/01/1901 - JavaScript mindate
										if (min <= (new Date("01/01/1901"))) {
											validatorRule += 'dateMax: \'' + max.toDateString() + '\', ';
											validatorMessage += 'dateMax: $.format(\'' + message + '\'), ';
										}
                                        //12/31/9999 - c# maxdate
										else if (max >= (new Date("12/31/9999"))) {
											validatorRule += 'dateMin: \'' + min.toDateString() + '\', ';
											validatorMessage += 'dateMin: $.format(\'' + message + '\'), ';
										}
										else {
											validatorRule += 'dateRange: [\'' + min.toDateString() + '\',\'' + max.toDateString() + '\'], ';
											validatorMessage += 'dateRange: $.format(\'' + message + '\'), ';
										}
									}
									else {
                                        var ruleName="range";
                                        if (exclusive==='true') ruleName="rangeEx";
                                        validatorRule += ruleName+': [' + min + ',' + max + '], ';
										validatorMessage += ruleName+': $.format(\'' + message + '\'), ';
									}
									break;
								case 'regularExpression':
									var regex = attributes[attribute].ValidationParameters.pattern;

									validatorRule += 'regex: \'' + regex.replace(/\\/g, '\\\\\\') + '\', ';
									validatorMessage += 'regex: $.format(\'' + message + '\'), ';

									break;
								case 'type':
									var type = attributes[attribute].ValidationParameters.typeName;

									validatorRule += type + ': true, ';
									validatorMessage += type + ': $.format(\'' + message + '\'), ';
									break;
                              case 'equalTo':
									var equalTo = attributes[attribute].ValidationParameters.equalTo;
                        
									validatorRule += 'equalTo: \'#'+equalTo+'\', ';;
									validatorMessage += 'equalTo: $.format(\'' + message + '\'), ';
									break;
							}
						}

						validatorRule += '}';
						validatorRule = validatorRule.replace(', }', '}');
						validatorRules += validatorRule + ', ';

						validatorMessage += '}';
						validatorMessage = validatorMessage.replace(', }', '}');
						validatorMessages += validatorMessage + ', ';
					}
					validatorRules += '}';
					validatorRules = validatorRules.replace(', }', '}');

					validatorMessages += '}';
					validatorMessages = validatorMessages.replace(', }', '}');

					var $form = $('#' + id);
					var validationExpression = '{' + validatorRules + ', ' + validatorMessages + '}';

					var json = eval('(' + validationExpression + ')');

					$form.validate(json);
				}

				o.callback();
			}
		});
	};

	$.fn.clientValidation.defaults = {
		initialize: function() {
		},
		isValid: function() {
			return true;
		},
		callback: function() {
		}
	};

})(jQuery);