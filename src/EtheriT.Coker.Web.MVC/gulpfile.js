var gulp = require('gulp');
var path = require('path');
var fs = require('fs');
var merge = require("merge-stream");

var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var uglifycss = require('gulp-uglifycss');
var less = require('gulp-less');


var bundleConfig = require(path.resolve(__dirname, 'bundles.json'));


gulp.task('clean', function () {
	console.log("in");
});

gulp.task('uglify:js', function () {
	return gulp.src('wwwroot/js/*.js')
		.pipe(concat('app.min.js'))
		.pipe(uglify())
		.pipe(gulp.dest('wwwroot/js'));
});

gulp.task('uglify:css', function () {
	return gulp.src('wwwroot/css/*.css')
		.pipe(concat('style.min.css'))
		.pipe(uglifycss())
		.pipe(gulp.dest('wwwroot/css'));
});

gulp.task('uglify', gulp.series('uglify:js', 'uglify:css'));