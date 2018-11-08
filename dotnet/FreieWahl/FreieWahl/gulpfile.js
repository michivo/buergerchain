/// <binding AfterBuild='copy' Clean='clean' />
"use strict";

const gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

const paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";

gulp.task("clean:js", done => rimraf(paths.concatJsDest, done));
gulp.task("clean:css", done => rimraf(paths.concatCssDest, done));
gulp.task("clean:dist", done => rimraf(paths.webroot + "lib", done));
gulp.task("clean", gulp.series(["clean:js", "clean:css" ]));

gulp.task("min:js", () => {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", () => {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", gulp.series(["min:js", "min:css"]));

// A 'default' task is required by Gulp v4
gulp.task("default", gulp.series(["min"]));

gulp.task('copy:jquery', function (done) {
    return gulp.src('./bower/lib/jquery/dist/jquery.min.js')
        .pipe(gulp.dest('./wwwroot/lib/jquery'));
});

gulp.task('copy', function (done) {
    return gulp.src(
        ['./bower/lib/jquery/dist/jquery.min.js',
            './bower/lib/jquery-ui/jquery-ui.min.js',
            './bower/lib/firebase/firebase-app.js',
            './bower/lib/firebase/firebase-auth.js',
            './bower/lib/bootstrap-datepicker/dist/css/bootstrap-datepicker3.standalone.min.css',
            './bower/lib/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js',
            './bower/lib/bootstrap-datepicker/dist/locales/bootstrap-datepicker.de.min.js',
            './bower/lib/slick-carousel/slick/slick.css',
            './bower/lib/slick-carousel/slick/slick-theme.css',
            './bower/lib/slick-carousel/slick/slick.js',
            './bower/lib/slick-carousel/slick/ajax-loader.gif',
            './bower/lib/slick-carousel/slick/fonts/slick.woff',
            './bower/lib/slick-carousel/slick/fonts/slick.ttf',
            './bower/lib/slick-carousel/slick/slick.min.js',
            './bower/lib/progressbar.js/dist/progressbar.min.js',
            './bower/lib/progressbar.js/dist/progressbar.min.js.map',
            './bower/lib/popper.js/dist/umd/popper.min.js'], { base: './bower/lib'})
        .pipe(gulp.dest('./wwwroot/lib'));
});