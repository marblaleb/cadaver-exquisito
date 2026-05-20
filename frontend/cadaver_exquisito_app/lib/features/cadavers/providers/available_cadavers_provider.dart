import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';

class CadaverSummary {
  final String id;
  final String title;
  final int maxParticipants;
  final int currentTurn;

  CadaverSummary({
    required this.id,
    required this.title,
    required this.maxParticipants,
    required this.currentTurn,
  });

  factory CadaverSummary.fromJson(Map<String, dynamic> j) => CadaverSummary(
        id: j['id'] as String,
        title: j['title'] as String,
        maxParticipants: j['maxParticipants'] as int,
        currentTurn: j['currentTurn'] as int,
      );
}

final availableCadaversProvider = FutureProvider<List<CadaverSummary>>((ref) async {
  final resp = await apiClient.get(Endpoints.availableCadavers);
  return (resp.data as List)
      .map((j) => CadaverSummary.fromJson(j as Map<String, dynamic>))
      .toList();
});

final pendingCadaversProvider = FutureProvider<List<CadaverSummary>>((ref) async {
  final resp = await apiClient.get(Endpoints.pendingCadavers);
  return (resp.data as List)
      .map((j) => CadaverSummary.fromJson(j as Map<String, dynamic>))
      .toList();
});
