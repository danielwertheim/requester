var gulp = require('gulp'),
    //debug = require('gulp-debug'),
    merge = require('merge-stream'),
    flatten = require('gulp-flatten'),
    shell = require('gulp-shell'),
    del = require('del'),
    msbuild = require('gulp-msbuild'),
    assemblyInfo = require('gulp-dotnet-assembly-info'),
    argv = require('yargs').argv,
    sequence = require('run-sequence');

var ver = '1.0.0',
    config = {
        src: './../src/',
        projects: ['Requester', 'Requester.Validation'],
        build: {
            outdir: './build/',
            version: ver,
            semver: ver + '-rc1',
            revision: argv && argv.buildrevision ? argv.buildrevision : '0',
            profile: argv.buildprofile || 'Release'
        }
    };

gulp.task('default', function (cb) {
    sequence(
        ['clean', 'assemblyinfo'],
        'build', 'copy', 'integration-tests', cb);
});

gulp.task('ci', function (cb) {
    sequence(
        ['init-tools', 'clean', 'assemblyinfo', 'nuget-restore'],
        'build', 'copy', 'integration-tests', 'nuget-pack', cb);
});

gulp.task('init-tools', shell.task([
    'nuget restore ./tools/packages.config -o ./tools/']
));

gulp.task('nuget-restore', function () {
    return gulp.src(config.src + '*.sln', { read: false })
        .pipe(shell('nuget restore <%= file.path %>'));
});

gulp.task('clean', function(cb) {
    del(config.build.outdir, cb);
});

gulp.task('assemblyinfo', function() {
    return gulp.src(config.src + 'GlobalAssemblyInfo.cs')
        .pipe(assemblyInfo({
          version: config.build.version + '.' + config.build.revision,
          informationalVersion: config.build.semver
        }))
        .pipe(gulp.dest(config.src));
});

gulp.task('build', function() {
    return gulp.src(config.src + '*.sln')
        .pipe(msbuild({
            toolsVersion: 14.0,
            configuration: config.build.profile,
            targets: ['Clean', 'Build'],
            errorOnFail: true,
            stdout: true,
            verbosity: 'minimal'
        }));
});

gulp.task('copy', function() {
    var tasks = config.projects.map(function (folder) {
        return gulp.src(config.src + 'projects/' + folder + '/bin/' + config.build.profile + '/*.{dll,XML}')
            .pipe(flatten())
            .pipe(gulp.dest(config.build.outdir + '/' + folder));
    });
});

gulp.task('integration-tests', function () {
    return gulp.src(config.src + 'tests/**/bin/' + config.build.profile + '/*.IntegrationTests.dll')
        .pipe(shell('xunit.console.exe <%= file.path %> -noshadow', { cwd: './tools/xunit.runner.console.2.1.0/tools/' }));
});

gulp.task('nuget-pack', function () {
    return gulp.src('*.nuspec', { read: false })
        .pipe(shell('nuget pack <%= file.path %> -version ' + config.build.semver + ' -basepath ' + config.build.outdir + ' -o ' + config.build.outdir));
});

gulp.task('nuget-pack2', shell.task([
    'nuget pack ./' + config.slnname + '.nuspec -version ' + config.build.semver + ' -basepath ' + config.build.outdir + ' -o ' + config.build.outdir]
));