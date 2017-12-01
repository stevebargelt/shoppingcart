#!/usr/bin/env groovy

import groovy.json.JsonOutput

node ('aspdotnetcore_shoppingcart') {
	try {
		notifyBuild('STARTED')
    notifyAtomist('STARTED', 'STARTED')
		stage('Build') {    
			git url: 'https://github.com/stevebargelt/shoppingcart'
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
    	notifyBuild(currentBuild.result)
      notifyAtomist(currentBuild.result, currentBuild.result)
	}

} //node

def notifyBuild(String buildStatus = 'STARTED') {
  // build status of null means successful
  buildStatus = buildStatus ?: 'SUCCESSFUL'
 
  // Default values
  def colorName = 'RED'
  def colorCode = '#FF0000'
  def subject = "Status=${buildStatus}, Job=${env.JOB_NAME}, Build=${env.BUILD_NUMBER}"
  def summary = "${subject}, URL=${env.BUILD_URL}"
  def details = """<p>${buildStatus}: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
    <p>Check console output at "<a href="${env.BUILD_URL}">${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>"</p>"""
 
  // Override default values based on build status
  if (buildStatus == 'STARTED') {
    color = 'YELLOW'
    colorCode = '#FFFF00'
  } else if (buildStatus == 'SUCCESSFUL') {
    color = 'GREEN'
    colorCode = '#36a64f'
  } else {
    color = 'RED'
    colorCode = '#FF0000'
  }
 
  // Send notifications
  slackSend (color: colorCode, message: summary)
   
}

def getSCMInformation() {
    def gitUrl = sh(returnStdout: true, script: 'git config --get remote.origin.url').trim()
    return [ url: gitUrl ]
}

def notifyAtomist(buildStatus, buildPhase="FINALIZED",
                  endpoint="https://webhook.atomist.com/atomist/jenkins/teams/T14LTGA75") {

    def payload = JsonOutput.toJson([
        name: env.JOB_NAME,
        duration: currentBuild.duration,
        build: [
            number: env.BUILD_NUMBER,
            phase: buildPhase,
            status: buildStatus,
            full_url: env.BUILD_URL,
            scm: getSCMInformation()
        ]
    ])
    sh "curl --silent -XPOST -H 'Content-Type: application/json' -d '${payload}' ${endpoint}"
}