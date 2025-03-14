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
