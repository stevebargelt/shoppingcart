#!/usr/bin/env groovy

import groovy.json.JsonOutput

node ('aspdotnetcore_shoppingcart') {
	try {
		git url: 'https://github.com/stevebargelt/shoppingcart'
    env.GITURL_ATOMIST = sh(returnStdout: true, script: 'git config --get remote.origin.url').trim()
    env.GITSHA_ATOMIST = sh(returnStdout: true, script: 'git rev-parse HEAD').trim()
    env.GITBRANCH_ATOMIST = sh(returnStdout: true, script: 'git name-rev --always --name-only HEAD').trim().replace('remotes/origin/', '')
    notifyAtomist('STARTED', 'STARTED')
		stage('Build') {    
			sh 'dotnet restore test/shoppingcart.Tests/shoppingcart.Tests.csproj'
			sh 'dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj'
		}
		stage('Publish') {
			sh 'dotnet publish src/shoppingcart/shoppingcart.csproj -c release -o $(pwd)/publish/'
			echo "Building: ${env.BUILD_TAG} || Build Number: ${env.BUILD_NUMBER}"
			sh "docker build -t abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER} publish"
			withCredentials([usernamePassword(credentialsId: 'absadmin', passwordVariable: 'REGISTRY_PASSWORD', usernameVariable: 'REGISTRY_USER')]) {
				sh "docker login abs-registry.harebrained-apps.com -u='${REGISTRY_USER}' -p='${REGISTRY_PASSWORD}'"
			}
				sh "docker push abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
		}
		stage('ABS-Test') {
			docker.withServer('tcp://abs.harebrained-apps.com:2376', 'dockerTLSCerts') {
				sh "docker pull abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
				sh "docker stop shoppingcart || true && docker rm shoppingcart || true"
				sh "docker run -d --name shoppingcart -p 8007:80 abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
			}
		}
	} catch (e) {
		currentBuild.result = "FAILED"
    throw e	
	} finally {
			// Success or failure, always send notifications
      notifyAtomist(currentBuild.result)
	}

} //node

@NonCPS
def notifyAtomist(buildStatus, buildPhase="FINALIZED",
                  endpoint="https://webhook.atomist.com/atomist/jenkins/teams/T14LTGA75") {

    // build status of null means successful
    buildStatus = buildStatus ?: 'SUCCESS'

    def payload = JsonOutput.toJson([
        name: env.JOB_NAME,
        duration: currentBuild.duration,
        build: [
            number: env.BUILD_NUMBER,
            phase: buildPhase,
            status: buildStatus,
            full_url: env.BUILD_URL,
            scm: [ url: env.GITURL_ATOMIST, branch: env.GITBRANCH_ATOMIST, commit: env.GITSHA_ATOMIST ]
        ]
    ])
    sh "curl --silent -XPOST -H \'Content-Type: application/json\' -d '${payload}' ${endpoint}"
}