SmartParking

Description du projet

SmartParking est une application de réservation de places de parking et de gestion de parking. Ce projet inclut une interface utilisateur pour les clients ainsi qu'une application d'administration permettant de superviser l'ensemble du système.

ADMIN :

Fonctionnalités

1. Vue d'ensemble

Affichage du nombre de places disponibles et occupées en temps réel.

Gestion des réservations des clients :

Visualisation des réservations en cours.

Suppression des réservations.

Détails des réservations : durée, date, immatriculation du véhicule.

2. Analyse des données des capteurs

Accès aux données environnementales du parking grâce aux capteurs :

Température.

Humidité.

Niveau sonore.

Taux de CO2.

Ces données sont récupérées en temps réel via MQTT.

3. Caméra de surveillance

Accès à un retour vidéo en direct des caméras de surveillance du parking.

4. Aide

Informations utiles pour l’administration du parking :

Numéros d'urgence.

Procédures en cas d'incident.

Technologies utilisées

Langage : C# (WPF)

API : Interaction avec une base de données pour la gestion des réservations.

Protocole MQTT :

Récupération des données des capteurs environnementaux.

Récupération de l'état des places de parking (occupées ou non) via des capteurs à ultrasons (données fournies par un collègue).

Vidéo : Intégration d'un flux vidéo pour la surveillance du parking


CLIENT:

Description
L'application mobile SmartParking permet aux utilisateurs de créer un compte, de se connecter, de réserver une place de parking et de consulter leur historique de réservation. Elle facilite ainsi la gestion des parkings de manière efficace et connectée.

Fonctionnalités

Création de compte : L'utilisateur peut s'inscrire avec ses informations personnelles.
Connexion : Authentification sécurisée pour accéder à son espace personnel.
Réservation de place : L'utilisateur peut réserver une place disponible dans le parking.
Consultation des réservations : Accès à l'historique des réservations effectuées.

Technologies utilisées

Framework : Flutter (Dart)
Base de données : API REST
Authentification : JWT
Gestion des requêtes : HTTP et API REST

PHYSIQUE:
