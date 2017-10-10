#!/usr/bin/env groovy

node ('aspdotnetcore_shoppingcart') {
	try {
		notifyBuild('STARTED')
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
	}

} //node

def notifyBuild(String buildStatus = 'STARTED') {
  // build status of null means successful
  buildStatus = buildStatus ?: 'SUCCESSFUL'
 
  // Default values
  def colorName = 'RED'
  def colorCode = '#FF0000'
  def subject = "${buildStatus}: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'"
  def summary = "${subject} (${env.BUILD_URL})"
  def details = """<p>STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
    <p>Check console output at "<a href="${env.BUILD_URL}">${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>"</p>"""
 
  // Override default values based on build status
  if (buildStatus == 'STARTED') {
    color = 'YELLOW'
    colorCode = '#FFFF00'
  } else if (buildStatus == 'SUCCESSFUL') {
    color = 'GREEN'
    colorCode = '#00FF00'
  } else {
    color = 'RED'
    colorCode = '#FF0000'
  }
 
  // Send notifications
  slackSend (color: colorCode, message: summary)
 
  // hipchatSend (color: color, notify: true, message: summary)
 
  // emailext (
  //     subject: subject,
  //     body: details,
  //     recipientProviders: [[$class: 'DevelopersRecipientProvider']]
  //   )
}