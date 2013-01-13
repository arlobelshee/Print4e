var fs = require('./file_system.js'),
	path = require('path');

module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		pkg: '<json:package.json>',
		meta: {
			banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
				'<%= grunt.template.today("yyyy-mm-dd") %>\n' +
				'<%= pkg.homepage ? "* " + pkg.homepage + "\n" : "" %>' +
				'* Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author.name %>;' +
				' Licensed <%= _.pluck(pkg.licenses, "type").join(", ") %> */'
		},
		coffeelint: {
			all: {
				files: ['lib/**/*.coffee', 'test/**/*.coffee', 'test_support/*.coffee'],
				options: {
					no_tabs: { level: "ignore" },
					indentation: { level: "ignore" },
					max_line_length: { level: "ignore" }
				}
			}
		},
		coffee: {
			all: {
				files: {
					'dist/Print4e.js': ['lib/**/*.coffee']
				}
			}
		},
		simplemocha: {
			all: {
				src: ['test_support/node.coffee', 'test/**/*.coffee'],
				options: {
					timeout: 500,
					ignoreLeaks: false,
					ui: 'bdd',
					reporter: 'progress',
					compilers: "coffee:coffee-script"
				}
			}
		},
		test: {
		},
		lint: {
			files: ['grunt.js', 'lib/**/*.js', 'test/**/*.js']
		},
		watch: {
			files: ['<config:lint.files>', '<config:coffeelint.all.files>'],
			tasks: 'default'
		},
		jshint: {
			options: {
				bitwise: true,
				curly: true,
				devel: true,
				eqeqeq: true,
				forin: true,
				funcscope: true,
				globalstrict: true,
				immed: true,
				indent: 3,
				latedef: true,
				maxlen: 160,
				newcap: true,
				noarg: true,
				node: true,
				nomen: true,
				nonew: true,
				plusplus: true,
				regexp: true,
				strict: false,
				trailing: true,
				undef: true,
				white: true
			},
			globals: {
				exports: true
			}
		}
	});

	// Load all the plugin tasks.
	grunt.loadNpmTasks('grunt-simple-mocha');
	grunt.loadNpmTasks('grunt-contrib-coffee');
	grunt.loadNpmTasks('grunt-coffeelint');
	grunt.renameTask('test', 'unused_node_test_tests');
	
	// Define more helper tasks
	grunt.registerTask('install_modules', function ()
	{
		var done = this.async();
		grunt.utils.spawn({
			cmd: "npm.cmd",
			args: ["install"]
		}, function (err)
		{
			if (err)
			{
				grunt.verbose.error();
				grunt.log.error(err);
				done(false);
				return;
			}
			grunt.verbose.ok();
			done();
		});
	});

	// Entry points
	grunt.registerTask('incremental', 'coffeelint coffee simplemocha lint');
	grunt.registerTask('full', 'install_modules incremental');
	grunt.registerTask('default', 'incremental');
};