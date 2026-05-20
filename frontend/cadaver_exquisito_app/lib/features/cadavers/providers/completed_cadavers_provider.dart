import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';

class CompletedCadaver {
  final String id;
  final String title;

  CompletedCadaver({required this.id, required this.title});

  factory CompletedCadaver.fromJson(Map<String, dynamic> j) =>
      CompletedCadaver(
        id: j['id'] as String,
        title: j['title'] as String,
      );
}

class FullFragment {
  final String content;
  final int sequenceOrder;
  final String authorName;

  FullFragment({
    required this.content,
    required this.sequenceOrder,
    required this.authorName,
  });

  factory FullFragment.fromJson(Map<String, dynamic> j) => FullFragment(
        content: j['content'] as String,
        sequenceOrder: j['sequenceOrder'] as int,
        authorName: j['authorName'] as String,
      );
}

final completedCadaversProvider =
    FutureProvider<List<CompletedCadaver>>((ref) async {
  final resp = await apiClient.get(Endpoints.completedCadavers);
  return (resp.data as List)
      .map((j) => CompletedCadaver.fromJson(j as Map<String, dynamic>))
      .toList();
});

final fullCadaverProvider =
    FutureProvider.family<List<FullFragment>, String>((ref, id) async {
  final resp = await apiClient.get(Endpoints.fullCadaver(id));
  return (resp.data['fragments'] as List)
      .map((j) => FullFragment.fromJson(j as Map<String, dynamic>))
      .toList();
});
