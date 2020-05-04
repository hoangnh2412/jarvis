/// <binding />
"use strict";

var gulp = require('gulp');
var inject = require("gulp-inject");
var browserSync = require('browser-sync').create();
var glob = require('glob');
var runSequence = require('run-sequence');
var path = require('path');
var historyApiFallback = require('connect-history-api-fallback');
var fs = require("fs");

//var templateCache = require('gulp-angular-templatecache');
//var htmlmin = require('gulp-htmlmin');
//var del = require('del');
//var concat = require('gulp-concat');
//var minify = require('gulp-minify');
//var cleanCss = require('gulp-clean-css');
//var rename = require('gulp-rename');
//var useref = require('gulp-useref');
//var gulpIf = require('gulp-if');
//var cssnano = require('gulp-cssnano');

var constants = {
    baseDir: 'wwwroot',
    source: 'wwwroot/app',
    application: () => {
        var path = process.cwd();
        var splited = path.split('/');
        var name = splited[splited.length - 1];
        return name;
    },
    port: 3000,
    views: 'Views',
    areas: 'Areas',
    themes: 'Themes',
    modules: 'Modules',
    jarvis: 'Jarvis'
};

gulp.task('default', () => {
    runSequence('build', 'serve');
});

gulp.task('serve', () => {
    browserSync.init({
        notify: false,
        port: constants.port,
        server: {
            baseDir: glob.sync('../**/' + constants.baseDir),
            middleware: [historyApiFallback()]
        }
    });
});

gulp.task('build', () => {
    var fileModules = [];
    var fileComponents = [];
    var fileRoutes = [];
    var ignores = [];

    //Scan các folder Modules dc cài đặt trong appsettings.json
    var modules = JSON.parse(fs.readFileSync('appsettings.json', 'utf-8')).Modules;
    for (var i = 0; i < modules.length; i++) {
        var files = glob.sync('../' + constants.modules + '/' + modules[i].Code + '/' + constants.baseDir + '/**/*.*');
        for (var j = 0; j < files.length; j++) {
            var file = files[j];
            ignores.push(file.substring(0, file.indexOf('/' + constants.baseDir + '/') + constants.baseDir.length + 1));

            if (file.endsWith('.module.js'))
                fileModules.push(file);

            if (file.endsWith('.component.js'))
                fileComponents.push(file);

            if (file.endsWith('.route.js'))
                fileRoutes.push(file);
        }
    }

    //Scan Jarvis để check version dc cài đặt trong appsettings.json
    var jarvis = JSON.parse(fs.readFileSync('appsettings.json', 'utf-8')).Jarvis;
    for (var i = 0; i < jarvis.length; i++) {
        var files = glob.sync('../' + constants.jarvis + '/' + jarvis[i].Code + '/' + constants.baseDir + '/**/*.*');
        for (var j = 0; j < files.length; j++) {
            const file = files[j];
            ignores.push(file.substring(0, file.indexOf('/' + constants.baseDir + '/') + constants.baseDir.length + 1));

            if (file.endsWith('.module.js'))
                fileModules.push(file);

            if (file.endsWith('.component.js'))
                fileComponents.push(file);

            if (file.endsWith('.route.js'))
                fileRoutes.push(file);
        }
    }

    return gulp
        .src(constants.baseDir + '/index.html')
        .pipe(inject(gulp.src(fileModules, { read: false }), {
            name: 'modules',
            ignorePath: ignores,
            addRootSlash: false
        }))
        .pipe(inject(gulp.src(fileComponents, { read: false }), {
            name: 'components',
            ignorePath: ignores,
            addRootSlash: false
        }))
        .pipe(inject(gulp.src(fileRoutes, { read: false }), {
            name: 'routes',
            ignorePath: ignores,
            addRootSlash: false
        }))
        .pipe(gulp.dest(constants.baseDir));
});

gulp.task('watch', () => {
    gulp.watch('wwwroot/**/*.*', () => {
        browserSync.reload();
    });

    gulp.watch('../' + constants.modules + '/**/wwwroot/**/*.*', () => {
        browserSync.reload();
    });

    gulp.watch('../' + constants.jarvis + '/**/wwwroot/**/*.*', () => {
        browserSync.reload();
    });
});

gulp.task('publish', () => {
    var fileModules = [];
    var fileServices = [];
    var fileComponents = [];

    var modules = JSON.parse(fs.readFileSync('appsettings.json', 'utf-8')).Modules;
    for (var i = 0; i < modules.length; i++) {
        var module = modules[i];
        var moduleName = module.Code.substring('Module.'.length, module.Code.length);
        var files = glob.sync('../' + constants.modules + '/' + module.Code + '/wwwroot/**/*.*');
        for (var j = 0; j < files.length; j++) {
            var file = files[j];

            var start = file.indexOf('/wwwroot/');
            var end = file.lastIndexOf('/');

            var fileName = file.substring(end + 1, file.length);
            //Use when publish to production
            //var splited = name.split('.');
            //splited[0] = splited[0] + '-' + Math.random().toString(36).substring(7);
            //name = splited.join('.');
            //console.log(name);

            var dest = './' + constants.source + '/' + moduleName.toLowerCase() + '/' + file.substring(start + 9, end);
            gulp.src(file).pipe(gulp.dest(dest));

            dest = './' + constants.source + '/' + moduleName.toLowerCase() + '/' + file.substring(start + 9, end) + '/' + fileName;
            if (file.endsWith('.module.js')) {
                fileModules.push(dest);
            }

            if (file.endsWith('.component.js')) {
                fileComponents.push(dest);
            }
        }
    }

    var jarvis = JSON.parse(fs.readFileSync('appsettings.json', 'utf-8')).Jarvis;
    for (var i = 0; i < jarvis.length; i++) {
        var module = jarvis[i];
        var files = glob.sync('../' + constants.jarvis + '/' + module.Code + '/wwwroot/**/*.*');
        var moduleName = module.Code.substring('Jarvis.'.length, module.Code.length);

        for (var j = 0; j < files.length; j++) {
            const file = files[j];
            var dest;

            var start = file.indexOf('/wwwroot/');
            var end = file.lastIndexOf('/');
            var fileName = file.substring(end + 1, file.length);

            if (module.Code === 'Jarvis.Core') {
                dest = './' + constants.source + '/' + file.substring(start + 8, end);
                gulp.src(file).pipe(gulp.dest(dest));

                dest = './' + constants.source + '/' + file.substring(start + 8, end) + '/' + fileName;
            } else {

                dest = './' + constants.source + '/' + moduleName.toLowerCase() + file.substring(start + 8, end);
                gulp.src(file).pipe(gulp.dest(dest));

                dest = './' + constants.source + '/' + moduleName.toLowerCase() + file.substring(start + 8, end) + '/' + fileName;
            }

            if (file.endsWith('.module.js')) {
                fileModules.push(dest);
            }

            if (file.endsWith('.service.js')) {
                fileServices.push(dest);
            }

            if (file.endsWith('.component.js')) {
                fileComponents.push(dest);
            }
        }
    }

    return gulp
        .src('wwwroot/index.html')
        .pipe(inject(gulp.src(fileModules, { read: false }), {
            name: 'modules',
            ignorePath: 'wwwroot',
            addRootSlash: false
        }))
        .pipe(inject(gulp.src(fileServices, { read: false }), {
            name: 'services',
            ignorePath: 'wwwroot',
            addRootSlash: false
        }))
        .pipe(inject(gulp.src(fileComponents, { read: false }), {
            name: 'components',
            ignorePath: 'wwwroot',
            addRootSlash: false
        }))
        .pipe(gulp.dest('wwwroot'));
});
